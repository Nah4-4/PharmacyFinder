# Pharmacy Finder

A full-stack pharmacy discovery application that helps customers find nearby pharmacies, search medicine availability, compare prices, and get directions.

Pharmacy Finder consists of:

- A cross-platform Flutter customer application
- An ASP.NET Core Web API
- A SQL Server database managed with Entity Framework Core
- JWT-based authentication
- Location-aware pharmacy and medicine searches
- Prescription image scanning with on-device OCR

## Features

### Customer Application

- Customer registration and login
- JWT authentication with local token persistence
- Search for medicines by name
- Find approved pharmacies near the user's location
- Filter results to show only medicines currently in stock
- Sort medicine results by:
  - Price
  - Distance
  - Available stock quantity
- View pharmacy details, including:
  - Pharmacy name
  - Address
  - Contact information
  - Operating hours
  - Medicine price and quantity
- Scan prescription images using:
  - Device camera
  - Gallery uploads
- Extract text from prescription images using Google ML Kit
- View pharmacies and medicine results on an interactive map
- Open turn-by-turn directions using Google Maps

### Backend API

- Customer authentication and registration
- Pharmacy and medicine search endpoints
- Location-based distance calculations
- Pharmacy approval filtering
- Pharmacy stock lookup
- SQL Server persistence using Entity Framework Core
- OpenAPI support for API exploration during development

## Technology Stack

### Frontend

- Flutter
- Dart
- `geolocator`
- `flutter_map`
- OpenStreetMap
- Google ML Kit Text Recognition
- `image_picker`
- `shared_preferences`
- `url_launcher`

### Backend

- ASP.NET Core Web API
- C#
- .NET 10
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- OpenAPI

## Repository Structure

```text
PharmacyFinder/
├── Flutter/
│   └── pharmacy_finder/
│       ├── lib/
│       │   ├── models/
│       │   ├── screens/
│       │   ├── services/
│       │   └── main.dart
│       ├── android/
│       ├── ios/
│       ├── linux/
│       ├── macos/
│       ├── web/
│       └── pubspec.yaml
│
└── backend/
    ├── PharmacyFinder.Api/
    │   ├── Controllers/
    │   ├── Data/
    │   ├── Dtos/
    │   ├── Enums/
    │   ├── Migrations/
    │   ├── Models/
    │   ├── Services/
    │   ├── Program.cs
    │   └── PharmacyFinder.Api.csproj
    ├── PharmacyFinder.slnx
    └── dotnet-tools.json
```

## Prerequisites

Install the following before running the project:

- Flutter SDK
- Dart SDK included with Flutter
- .NET SDK 10
- SQL Server or SQL Server LocalDB
- Android Studio, Xcode, or another Flutter-supported development environment
- A device or emulator with location permissions enabled

Verify your installations:

```bash
flutter doctor
dotnet --version
```

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Nah4-4/PharmacyFinder.git
cd PharmacyFinder
```

### 2. Configure the Database

The backend uses a SQL Server connection string configured in:

```text
backend/PharmacyFinder.Api/appsettings.json
```

The default development connection string uses SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "PharmacyFinder": "Server=(localdb)\\mssqllocaldb;Database=PharmacyFinder;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Update this value if you are using a different SQL Server instance.

### 3. Apply Database Migrations

From the repository root:

```bash
cd backend/PharmacyFinder.Api
dotnet restore
dotnet ef database update
```

If the Entity Framework CLI is not installed, install it with:

```bash
dotnet tool install --global dotnet-ef
```

### 4. Run the Backend API

From `backend/PharmacyFinder.Api`:

```bash
dotnet run
```

The API will start using the URLs configured in:

```text
backend/PharmacyFinder.Api/Properties/launchSettings.json
```

Keep the API running while using the Flutter application.

### 5. Configure the Flutter Application

Open:

```text
Flutter/pharmacy_finder/lib/services/api_service.dart
```

Update `baseUrl` so it points to the API address reachable by your device or emulator.

Example:

```dart
static const String baseUrl = 'http://192.168.1.3:5000/api';
```

Common development addresses:

| Environment | API address |
| --- | --- |
| Android emulator | `http://10.0.2.2:5000/api` |
| iOS simulator | `http://localhost:5000/api` |
| Physical device | `http://YOUR_COMPUTER_IP:5000/api` |
| Web browser | `http://localhost:5000/api` |

Make sure the device and development computer are connected to the same network when using a physical device.

### 6. Run the Flutter Application

From the Flutter project directory:

```bash
cd Flutter/pharmacy_finder
flutter pub get
flutter run
```

To see available devices:

```bash
flutter devices
```

## API Endpoints

The API is organized under the `/api` route.

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
```

### Medicine Search

```http
GET /api/search/medicines
```

Supported query parameters include:

- `query`
- `lat`
- `lng`
- `maxDistanceKm`
- `inStockOnly`
- `sortBy`

Example:

```http
GET /api/search/medicines?query=paracetamol&lat=9.0104&lng=38.7445&inStockOnly=true&sortBy=price
```

### Pharmacy Search

```http
GET /api/search/pharmacies
```

Optional query parameters:

- `lat`
- `lng`

Example:

```http
GET /api/search/pharmacies?lat=9.0104&lng=38.7445
```

### Pharmacy Stock

```http
GET /api/search/pharmacies/{id}/stocks
```

## Location and Maps

The application uses the device's current location to:

- Find nearby approved pharmacies
- Calculate approximate pharmacy distance
- Sort results by distance
- Display the user's location on a map
- Display pharmacy locations
- Open external turn-by-turn directions

Map tiles are provided by OpenStreetMap. Please retain the required OpenStreetMap attribution when deploying the application.

## Prescription Scanning

Prescription scanning is performed locally on the device using Google ML Kit Text Recognition.

The current workflow:

1. Select an image from the camera or gallery.
2. Extract text from the image.
3. Identify medicine-like words and dosages.
4. Use an extracted word to start a medicine search.

Prescription images and OCR results are not currently uploaded to or stored by the backend.

## Development Notes

- Only approved pharmacies are returned by public search endpoints.
- Distance calculations use latitude and longitude coordinates.
- The Flutter application requests location permission at runtime.
- API authentication uses JWT tokens.
- Database schema changes are managed through Entity Framework Core migrations.
- The project currently focuses on the customer pharmacy-search workflow.

## Building for Release

### Android

```bash
cd Flutter/pharmacy_finder
flutter build apk --release
```

### Web

```bash
cd Flutter/pharmacy_finder
flutter build web
```

### Linux

```bash
cd Flutter/pharmacy_finder
flutter build linux
```

Additional platform configuration may be required for location, camera, and image permissions before creating a production release.

## Current Limitations

- Prescription images are processed locally and are not stored.
- The customer interface supports distance sorting, but does not currently expose a maximum-distance input.
- The API connection URL is currently configured directly in the Flutter source code.
- Production deployment requires secure HTTPS configuration, environment-specific secrets, and a production database.
- Camera and location permissions must be configured for each target platform.

## License

No license has been specified for this repository yet. Add a license before distributing or reusing the project publicly.

## Author

Developed by [Nah4-4](https://github.com/Nah4-4).