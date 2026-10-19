class MedicineStockResult {
  final int stockId;
  final int medicineId;
  final String name;
  final double price;
  final int quantity;
  final bool inStock;
  final int pharmacyId;
  final String pharmacyName;
  final String address;
  final String contact;
  final double latitude;
  final double longitude;
  final String operatingHours;
  final double? distanceKm;

  MedicineStockResult({
    required this.stockId,
    required this.medicineId,
    required this.name,
    required this.price,
    required this.quantity,
    required this.inStock,
    required this.pharmacyId,
    required this.pharmacyName,
    required this.address,
    required this.contact,
    required this.latitude,
    required this.longitude,
    required this.operatingHours,
    this.distanceKm,
  });

  factory MedicineStockResult.fromJson(Map<String, dynamic> json) {
    return MedicineStockResult(
      stockId: json['stockId'],
      medicineId: json['medicineId'],
      name: json['name'] ?? '',
      price: (json['price'] as num).toDouble(),
      quantity: json['quantity'] ?? 0,
      inStock: json['inStock'] ?? false,
      pharmacyId: json['pharmacyId'],
      pharmacyName: json['pharmacyName'] ?? '',
      address: json['address'] ?? '',
      contact: json['contact'] ?? '',
      latitude: (json['latitude'] as num).toDouble(),
      longitude: (json['longitude'] as num).toDouble(),
      operatingHours: json['operatingHours'] ?? '',
      distanceKm: json['distanceKm'] != null
          ? (json['distanceKm'] as num).toDouble()
          : null,
    );
  }

}

class Pharmacy {
  final int id;
  final String name;
  final String address;
  final String contactPhone;
  final double latitude;
  final double longitude;
  final String operatingHours;
  final double? distanceKm;

  Pharmacy({
    required this.id,
    required this.name,
    required this.address,
    required this.contactPhone,
    required this.latitude,
    required this.longitude,
    required this.operatingHours,
    this.distanceKm,
  });

  factory Pharmacy.fromJson(Map<String, dynamic> json) {
    return Pharmacy(
      id: json['id'],
      name: json['name'] ?? '',
      address: json['address'] ?? '',
      contactPhone: json['contactPhone'] ?? '',
      latitude: (json['latitude'] as num).toDouble(),
      longitude: (json['longitude'] as num).toDouble(),
      operatingHours: json['operatingHours'] ?? '',
      distanceKm: json['distanceKm'] != null
          ? (json['distanceKm'] as num).toDouble()
          : null,
    );
  }
}