import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import 'package:image_picker/image_picker.dart';
import 'package:google_mlkit_text_recognition/google_mlkit_text_recognition.dart';

import '../services/api_service.dart';
import '../models/models.dart';
import 'map_screen.dart';
import 'login_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final _searchCtrl = TextEditingController();
  List<MedicineStockResult> _results = [];
  List<Pharmacy> _pharmacies = [];
  List<String> _extractedWords = [];

  bool _loading = false;
  bool _showPharmacies = true;
  bool _inStockOnly = true;
  String _sortBy = 'price';
  double? _userLat;
  double? _userLng;

  @override
  void initState() {
    super.initState();
    _initLocationAndLoadPharmacies();
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  Future<void> _initLocationAndLoadPharmacies() async {
    try {
      LocationPermission perm = await Geolocator.checkPermission();
      if (perm == LocationPermission.denied) {
        perm = await Geolocator.requestPermission();
      }
      if (perm == LocationPermission.whileInUse ||
          perm == LocationPermission.always) {
        final pos = await Geolocator.getCurrentPosition();
        _userLat = pos.latitude;
        _userLng = pos.longitude;
      }
    } catch (_) {
      _userLat = 9.0104;
      _userLng = 38.7445;
    }
    await _loadPharmacies();
  }

  Future<void> _loadPharmacies() async {
    setState(() => _loading = true);
    try {
      final data = await ApiService.getPharmacies(lat: _userLat, lng: _userLng);
      if (mounted) setState(() => _pharmacies = data);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Pharmacy load error: $e')));
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  Future<void> _runSearch() async {
    setState(() => _showPharmacies = false);
    setState(() => _loading = true);
    try {
      final data = await ApiService.searchMedicines(
        query: _searchCtrl.text,
        lat: _userLat,
        lng: _userLng,
        inStockOnly: _inStockOnly,
        sortBy: _sortBy,
      );
      setState(() => _results = data);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Search error: $e')));
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  // Pick prescription image & extract medicine words locally with OCR
  Future<void> _scanPrescription(ImageSource source) async {
    final picker = ImagePicker();
    final pickedFile = await picker.pickImage(source: source);
    if (pickedFile == null) return;

    setState(() => _loading = true);
    try {
      final inputImage = InputImage.fromFilePath(pickedFile.path);
      final textRecognizer = TextRecognizer();
      final RecognizedText recognizedText = await textRecognizer.processImage(
        inputImage,
      );
      await textRecognizer.close();
      // Extract tokens while preserving medicine names such as "Co-Amoxiclav"
      // and dosages such as "500mg".
      final tokenPattern = RegExp(r'[A-Za-z0-9]+(?:[-/][A-Za-z0-9]+)*');
      final words = tokenPattern
          .allMatches(recognizedText.text)
          .map((match) => match.group(0)!)
          .where(
            (word) => word.length >= 2 && RegExp(r'[A-Za-z]').hasMatch(word),
          )
          .toSet()
          .toList();

      setState(() {
        _extractedWords = words;
        if (words.isNotEmpty) {
          _searchCtrl.text = words.first;
        }
      });

      await _runSearch();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('OCR failed: $e')));
      }
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;

    return Scaffold(
      backgroundColor: colors.surfaceContainerLowest,
      appBar: AppBar(
        titleSpacing: 20,
        title: const Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Pharmacy Finder',
              style: TextStyle(fontWeight: FontWeight.w700),
            ),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Log out',
            onPressed: () async {
              await ApiService.logout();
              if (!context.mounted) return;
              Navigator.pushReplacement(
                context,
                MaterialPageRoute(builder: (_) => const LoginScreen()),
              );
            },
          ),
        ],
      ),
      body: Column(
        children: [
          // 1. Search Bar & Prescription Scanner Buttons
          Container(
            width: double.infinity,
            padding: const EdgeInsets.fromLTRB(20, 18, 20, 16),
            decoration: BoxDecoration(
              color: colors.surface,
              border: Border(bottom: BorderSide(color: colors.outlineVariant)),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  _showPharmacies
                      ? 'What are you looking for?'
                      : 'Medicine search',
                  style: Theme.of(context).textTheme.titleMedium
                      ?.copyWith(fontWeight: FontWeight.w700),
                ),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _searchCtrl,
                        textInputAction: TextInputAction.search,
                        decoration: InputDecoration(
                          hintText: 'Search by medicine name',
                          prefixIcon: const Icon(Icons.search),
                          suffixIcon: _searchCtrl.text.isEmpty
                              ? null
                              : IconButton(
                                  icon: const Icon(Icons.clear),
                                  tooltip: 'Clear search',
                                  onPressed: () {
                                    _searchCtrl.clear();
                                    _loadPharmacies();
                                    setState(() => _showPharmacies = true);
                                  },
                                ),
                          filled: true,
                          fillColor: colors.surfaceContainerHighest,
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(16),
                            borderSide: BorderSide.none,
                          ),
                        ),
                        onChanged: (_) => setState(() {}),
                        onSubmitted: (_) => _runSearch(),
                      ),
                    ),
                    const SizedBox(width: 8),
                    IconButton.filled(
                      icon: const Icon(Icons.camera_alt_outlined),
                      tooltip: 'Scan prescription with camera',
                      onPressed: () => _scanPrescription(ImageSource.camera),
                    ),
                    const SizedBox(width: 4),
                    IconButton.filledTonal(
                      icon: const Icon(Icons.photo_library_outlined),
                      tooltip: 'Upload prescription photo',
                      onPressed: () => _scanPrescription(ImageSource.gallery),
                    ),
                  ],
                ),
                if (_showPharmacies) ...[
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Icon(
                        Icons.location_on_outlined,
                        size: 17,
                        color: colors.primary,
                      ),
                      const SizedBox(width: 4),
                      Text(
                        'Pharmacies near your location',
                        style: TextStyle(
                          color: colors.onSurfaceVariant,
                          fontSize: 13,
                        ),
                      ),
                    ],
                  ),
                ],
              ],
            ),
          ),

          // 2. Extracted Prescription Words (Tap a chip to search that medicine)
          if (_extractedWords.isNotEmpty)
            SizedBox(
              height: 56,
              child: ListView.separated(
                padding: const EdgeInsets.fromLTRB(20, 10, 20, 8),
                scrollDirection: Axis.horizontal,
                itemCount: _extractedWords.length,
                separatorBuilder: (_, _) => const SizedBox(width: 6),
                itemBuilder: (context, index) {
                  final word = _extractedWords[index];
                  return ActionChip(
                    label: Text(word),
                    onPressed: () {
                      _searchCtrl.text = word;
                      _runSearch();
                    },
                  );
                },
              ),
            ),

          // 3. Medicine filters are shown after a medicine search starts.
          if (!_showPharmacies)
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 12, 20, 8),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      Text(
                        'Sort by',
                        style: TextStyle(
                          color: colors.onSurfaceVariant,
                          fontSize: 13,
                        ),
                      ),
                      const SizedBox(width: 6),
                      DropdownButtonHideUnderline(
                        child: DropdownButton<String>(
                          value: _sortBy,
                          borderRadius: BorderRadius.circular(12),
                          items: const [
                            DropdownMenuItem(
                              value: 'price',
                              child: Text('Price'),
                            ),
                            DropdownMenuItem(
                              value: 'distance',
                              child: Text('Distance'),
                            ),
                            DropdownMenuItem(
                              value: 'availability',
                              child: Text('Stock Qty'),
                            ),
                          ],
                          onChanged: (val) {
                            if (val != null) {
                              setState(() => _sortBy = val);
                              _runSearch();
                            }
                          },
                        ),
                      ),
                    ],
                  ),
                  Row(
                    children: [
                      Text(
                        'In stock',
                        style: TextStyle(
                          color: colors.onSurfaceVariant,
                          fontSize: 13,
                        ),
                      ),
                      Switch.adaptive(
                        value: _inStockOnly,
                        onChanged: (val) {
                          setState(() => _inStockOnly = val);
                          _runSearch();
                        },
                      ),
                    ],
                  ),
                ],
              ),
            ),

          const Divider(height: 1),

          // 4. Medicine & Pharmacy Results List
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : _showPharmacies
                ? _pharmacies.isEmpty
                      ? _EmptyState(
                          icon: Icons.local_pharmacy_outlined,
                          title: 'No pharmacies found',
                          message: 'We could not find pharmacies near you.',
                        )
                      : ListView.builder(
                          padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
                          itemCount: _pharmacies.length,
                          itemBuilder: (context, index) {
                            final pharmacy = _pharmacies[index];
                            return Card(
                              margin: const EdgeInsets.only(bottom: 12),
                              elevation: 0,
                              clipBehavior: Clip.antiAlias,
                              child: ListTile(
                                contentPadding: const EdgeInsets.symmetric(
                                  horizontal: 16,
                                  vertical: 8,
                                ),
                                leading: CircleAvatar(
                                  backgroundColor: colors.primaryContainer,
                                  foregroundColor: colors.onPrimaryContainer,
                                  child: const Icon(
                                    Icons.local_pharmacy_outlined,
                                  ),
                                ),
                                title: Text(
                                  pharmacy.name,
                                  style: const TextStyle(
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                subtitle: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const SizedBox(height: 4),
                                    Text(
                                      pharmacy.address,
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    Text(
                                      '${pharmacy.contactPhone} • ${pharmacy.operatingHours}',
                                      maxLines: 1,
                                      overflow: TextOverflow.ellipsis,
                                    ),
                                    if (pharmacy.distanceKm != null)
                                      Text(
                                        '${pharmacy.distanceKm} km away',
                                        style: TextStyle(color: colors.primary),
                                      ),
                                  ],
                                ),
                                trailing: IconButton.filledTonal(
                                  tooltip: 'View on map',
                                  onPressed: () {
                                    Navigator.push(
                                      context,
                                      MaterialPageRoute(
                                        builder: (_) => MapScreen.forPharmacy(
                                          pharmacy: pharmacy,
                                          userLat: _userLat ?? 9.0104,
                                          userLng: _userLng ?? 38.7445,
                                        ),
                                      ),
                                    );
                                  },
                                  icon: const Icon(Icons.map_outlined),
                                ),
                              ),
                            );
                          },
                        )
                : _results.isEmpty
                ? _EmptyState(
                    icon: Icons.search_off,
                    title: 'No matching medicines',
                    message: 'Try another name or scan a prescription.',
                  )
                : ListView.builder(
                    padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
                    itemCount: _results.length,
                    itemBuilder: (context, index) {
                      final item = _results[index];
                      return Card(
                        margin: const EdgeInsets.only(bottom: 12),
                        elevation: 0,
                        clipBehavior: Clip.antiAlias,
                        child: ListTile(
                          contentPadding: const EdgeInsets.fromLTRB(
                            16,
                            12,
                            12,
                            12,
                          ),
                          leading: CircleAvatar(
                            backgroundColor: colors.secondaryContainer,
                            foregroundColor: colors.onSecondaryContainer,
                            child: const Icon(Icons.medication_outlined),
                          ),
                          title: Text(
                            item.name,
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                          subtitle: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(item.pharmacyName),
                              Text('${item.address} • ${item.operatingHours}'),
                              Text(
                                '${item.inStock ? "In Stock" : "Out of Stock"}  |  ${item.distanceKm != null ? "${item.distanceKm} km away" : ""}',
                                style: TextStyle(
                                  color: item.inStock
                                      ? Colors.green[700]
                                      : Colors.red,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ],
                          ),
                          trailing: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Text(
                                '${item.price.toStringAsFixed(0)} ETB',
                                style: const TextStyle(
                                  fontSize: 15,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              const SizedBox(height: 4),
                              InkWell(
                                onTap: () {
                                  Navigator.push(
                                    context,
                                    MaterialPageRoute(
                                      builder: (_) => MapScreen(
                                        item: item,
                                        userLat: _userLat ?? 9.0104,
                                        userLng: _userLng ?? 38.7445,
                                      ),
                                    ),
                                  );
                                },
                                child: const Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Icon(
                                      Icons.directions,
                                      size: 25,
                                      color: Colors.blue,
                                    ),
                                    Text(
                                      'Map',
                                      style: TextStyle(
                                        color: Colors.blue,
                                        fontSize: 15,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                            ],
                          ),
                        ),
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }
}

class _EmptyState extends StatelessWidget {
  final IconData icon;
  final String title;
  final String message;

  const _EmptyState({
    required this.icon,
    required this.title,
    required this.message,
  });

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, size: 52, color: colors.primary),
            const SizedBox(height: 16),
            Text(
              title,
              style: Theme.of(context).textTheme.titleLarge
                  ?.copyWith(fontWeight: FontWeight.w700),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            Text(
              message,
              style: TextStyle(color: colors.onSurfaceVariant),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}
