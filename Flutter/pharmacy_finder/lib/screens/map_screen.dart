import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:url_launcher/url_launcher.dart';

import '../models/models.dart';

class MapScreen extends StatelessWidget {
  final MedicineStockResult? item;
  final Pharmacy? pharmacy;
  final double userLat;
  final double userLng;

  const MapScreen({
    super.key,
    required this.item,
    required this.userLat,
    required this.userLng,
  }) : pharmacy = null;

  const MapScreen.forPharmacy({
    super.key,
    required this.pharmacy,
    required this.userLat,
    required this.userLng,
  }) : item = null;

  String get _name => item?.pharmacyName ?? pharmacy!.name;
  String get _address => item?.address ?? pharmacy!.address;
  String get _contact => item?.contact ?? pharmacy!.contactPhone;
  double get _latitude => item?.latitude ?? pharmacy!.latitude;
  double get _longitude => item?.longitude ?? pharmacy!.longitude;
  String get _operatingHours =>
      item?.operatingHours ?? pharmacy!.operatingHours;

  Future<void> _openTurnByTurnDirections(BuildContext context) async {
    final url = Uri.parse(
      'https://www.google.com/maps/dir/?api=1&origin=$userLat,$userLng&destination=$_latitude,$_longitude&travelmode=driving',
    );
    final launched = await launchUrl(url, mode: LaunchMode.externalApplication);
    if (!launched && context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'No browser or maps app is available to open directions.',
          ),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final userPoint = LatLng(userLat, userLng);
    final pharmacyPoint = LatLng(_latitude, _longitude);

    return Scaffold(
      appBar: AppBar(title: Text(_name)),
      body: Column(
        children: [
          Expanded(
            child: FlutterMap(
              options: MapOptions(
                initialCenter: pharmacyPoint,
                initialZoom: 13.5,
              ),
              children: [
                TileLayer(
                  urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                  userAgentPackageName: 'com.example.pharmacy_finder_app',
                ),
                RichAttributionWidget(
                  attributions: [
                    TextSourceAttribution(
                      'OpenStreetMap contributors',
                      onTap: () => launchUrl(
                        Uri.parse('https://www.openstreetmap.org/copyright'),
                        mode: LaunchMode.externalApplication,
                      ),
                    ),
                  ],
                ),
                PolylineLayer(
                  polylines: [
                    Polyline(
                      points: [userPoint, pharmacyPoint],
                      strokeWidth: 4.0,
                      color: Colors.blue,
                    ),
                  ],
                ),
                MarkerLayer(
                  markers: [
                    Marker(
                      point: userPoint,
                      width: 40,
                      height: 40,
                      child: const Icon(
                        Icons.my_location,
                        color: Colors.blue,
                        size: 32,
                      ),
                    ),
                    Marker(
                      point: pharmacyPoint,
                      width: 40,
                      height: 40,
                      child: const Icon(
                        Icons.local_pharmacy,
                        color: Colors.red,
                        size: 36,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
          Container(
            padding: const EdgeInsets.all(16),
            color: Colors.white,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  _name,
                  style: const TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text('$_address • Phone: $_contact'),
                Text('Hours: $_operatingHours'),
                const SizedBox(height: 12),
                ElevatedButton.icon(
                  onPressed: () => _openTurnByTurnDirections(context),
                  icon: const Icon(Icons.navigation),
                  label: const Text('Start Turn-by-Turn Directions'),
                  style: ElevatedButton.styleFrom(
                    minimumSize: const Size.fromHeight(48),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
