import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

import '../models/models.dart';

class ApiService {
  static const String baseUrl = 'http://192.168.1.3:5000/api';

  static Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('jwt_token');
  }

  static Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('jwt_token');
    await prefs.remove('user_name');
  }

  // 1. Login
  static Future<String?> login(String email, String password) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );

    final body = jsonDecode(response.body);
    if (response.statusCode == 200 && body['success'] == true) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('jwt_token', body['data']['token']);
      await prefs.setString('user_name', body['data']['fullName']);
      return null; // null means no error
    }
    return body['message'] ?? 'Login failed';
  }

  // 2. Register Customer
  static Future<String?> register(
    String fullName,
    String email,
    String password,
  ) async {
    final response = await http.post(
      Uri.parse('$baseUrl/auth/register'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'fullName': fullName,
        'email': email,
        'password': password,
        'role': 'Customer',
      }),
    );

    final body = jsonDecode(response.body);
    if ((response.statusCode == 200 || response.statusCode == 201) &&
        body['success'] == true) {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('jwt_token', body['data']['token']);
      await prefs.setString('user_name', body['data']['fullName']);
      return null;
    }
    return body['message'] ?? 'Registration failed';
  }

  // 3. Search Medicines with Filters
  static Future<List<MedicineStockResult>> searchMedicines({
    String? query,
    double? lat,
    double? lng,
    bool inStockOnly = false,
    String sortBy = 'price',
  }) async {
    final queryParams = <String, String>{
      'inStockOnly': inStockOnly.toString(),
      'sortBy': sortBy,
    };

    if (query != null && query.trim().isNotEmpty) {
      queryParams['query'] = query.trim();
    }
    if (lat != null && lng != null) {
      queryParams['lat'] = lat.toString();
      queryParams['lng'] = lng.toString();
    }

    final uri = Uri.parse('$baseUrl/search/medicines')
        .replace(queryParameters: queryParams);

    final token = await getToken();
    final response = await http.get(
      uri,
      headers: {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      },
    );

    if (response.statusCode == 200) {
      final body = jsonDecode(response.body);
      final List data = body['data'] ?? [];
      return data.map((e) => MedicineStockResult.fromJson(e)).toList();
    }
    if (response.statusCode == 404) {
      return [];
    }
    throw Exception('Failed to load medicines');
  }

  static Future<List<Pharmacy>> getPharmacies({
    double? lat,
    double? lng,
  }) async {
    final queryParams = <String, String>{};
    if (lat != null && lng != null) {
      queryParams['lat'] = lat.toString();
      queryParams['lng'] = lng.toString();
    }

    final uri = Uri.parse('$baseUrl/search/pharmacies')
        .replace(queryParameters: queryParams);

    final token = await getToken();
    final response = await http.get(
      uri,
      headers: {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': '******',
      },
    );

    if (response.statusCode == 200) {
      final body = jsonDecode(response.body);
      final List data = body['data'] ?? [];
      return data.map((e) => Pharmacy.fromJson(e)).toList();
    }
    if (response.statusCode == 404) {
      return [];
    }
    throw Exception('Failed to load pharmacies');
  }
}
