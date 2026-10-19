# Pharmacy Finder

Pharmacy Finder is a Flutter customer application backed by an ASP.NET Core
Web API. Customers can find approved pharmacies and search their medicine
stock using their current location.

## Customer feature assessment

| Customer requirement | Status | Implementation |
| --- | --- | --- |
| Customer registration and authentication | Complete | Flutter registration/login screens use the API and persist the JWT locally. |
| Search for medicines | Complete | Customers can search medicine names through the API. |
| Find pharmacies by location | Complete | The app requests device location and displays approved pharmacies ordered by distance. |
| Filter by availability | Complete | The **In Stock Only** filter is sent to the API. |
| Sort by price, distance, or stock quantity | Complete | The results screen supports all three sort options. |
| Prescription image upload and medicine extraction | Partial | Camera/gallery selection and on-device OCR are implemented. The image and extracted prescription are not uploaded or stored by the API. |
| Display pharmacy details | Complete | Results show the pharmacy name, address, contact number, operating hours, stock quantity, and price. |
| Display directions | Complete | A map screen shows the customer and pharmacy locations and can open turn-by-turn directions in Google Maps. |
| Distance filtering | Partial | The API supports a maximum-distance query, but the Flutter customer UI currently provides distance sorting rather than a maximum-distance input. |

### Customer conclusion

The project satisfies the main customer journey: a customer can register or
log in, search for an available medicine, compare pharmacy results, and open
directions to a pharmacy. It should be described as **mostly complete for the
customer part**, with prescription storage and a visible maximum-distance
filter still needed for full requirement coverage.

## Technology stack

- **Frontend:** Flutter (Dart)
- **Backend:** ASP.NET Core Web API
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** JWT tokens
- **Prescription processing:** Flutter image picker and on-device Google ML Kit OCR
- **Maps and directions:** Flutter Map/OpenStreetMap with external Google Maps directions

The frontend requirement for this project is **Flutter**.

## Running the Flutter app

1. Start the ASP.NET Core API.
2. Update `lib/services/api_service.dart` with the API address reachable from
   the target device or emulator.
3. Run:

```bash
flutter pub get
flutter run
```
