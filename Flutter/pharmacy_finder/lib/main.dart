import 'package:flutter/material.dart';
import './screens/login_screen.dart';

void main() {
  runApp(const PharmacyFinderApp());
}

class PharmacyFinderApp extends StatelessWidget {
  const PharmacyFinderApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Pharmacy Finder',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorSchemeSeed: Colors.teal,
        useMaterial3: true,
      ),
      home: const LoginScreen(),
    );
  }
}