# Photographer Style API Documentation

## Overview
This document describes the API endpoints for managing photographer styles in the Snapdi application. The system allows photographers to select multiple styles that match their expertise, and provides functionality for administrators to manage available styles.

## Features
- **Style Management**: Create, read, update, and delete photography styles
- **Photographer Style Assignment**: Assign multiple styles to photographers
- **Default Styles**: Pre-defined set of common photography styles
- **Style Selection Interface**: Get all styles with selection status for photographers

## API Endpoints

### Style Management

#### Get All Styles
```
GET /api/styles
```
**Description**: Get all available photography styles
**Authentication**: Public
**Response**: Array of StyleDto objects

#### Get Style by ID
```
GET /api/styles/{id}
```
**Description**: Get a specific style by ID
**Authentication**: Public
**Parameters**: 
- `id` (int): Style ID

#### Create Style
```
POST /api/styles
```
**Description**: Create a new photography style (Admin only)
**Authentication**: Admin required
**Body**: CreateStyleDto
```json
{
  "styleName": "Wedding Photography"
}
```

#### Update Style
```
PUT /api/styles/{id}
```
**Description**: Update an existing style (Admin only)
**Authentication**: Admin required
**Body**: UpdateStyleDto
```json
{
  "styleName": "Updated Style Name"
}
```

#### Delete Style
```
DELETE /api/styles/{id}
```
**Description**: Delete a style (Admin only)
**Authentication**: Admin required

### Photographer Style Management

#### Get Photographer Styles
```
GET /api/photographerstyles/photographer/{userId}
```
**Description**: Get all styles assigned to a specific photographer
**Authentication**: Public
**Response**: Array of StyleDto objects

#### Get Photographer with Styles
```
GET /api/photographerstyles/photographer/{userId}/with-styles
```
**Description**: Get photographer information with their selected styles
**Authentication**: Public
**Response**: PhotographerStyleResponseDto

#### Get Style Selection for Photographer
```
GET /api/photographerstyles/photographer/{userId}/selection
```
**Description**: Get all available styles with selection status for a photographer
**Authentication**: Authenticated users (own data or admin)
**Response**: Array of StyleSelectionDto objects

#### Add Style to Photographer
```
POST /api/photographerstyles/photographer/{userId}/style/{styleId}
```
**Description**: Add a style to a photographer
**Authentication**: Authenticated users (own data or admin)

#### Remove Style from Photographer
```
DELETE /api/photographerstyles/photographer/{userId}/style/{styleId}
```
**Description**: Remove a style from a photographer
**Authentication**: Authenticated users (own data or admin)

#### Update Photographer Styles
```
PUT /api/photographerstyles/photographer/{userId}/styles
```
**Description**: Replace all styles for a photographer
**Authentication**: Authenticated users (own data or admin)
**Body**: Array of style IDs
```json
[1, 2, 3, 4]
```

#### Get Photographers by Style
```
GET /api/photographerstyles/style/{styleId}/photographers
```
**Description**: Get all photographers who have a specific style
**Authentication**: Public
**Response**: Array of PhotographerStyleDto objects

### Admin Functions

#### Seed Default Styles
```
POST /api/admin/seed-default-styles
```
**Description**: Seed the database with default photography styles
**Authentication**: Admin required

## Data Transfer Objects (DTOs)

### StyleDto
```json
{
  "styleId": 1,
  "styleName": "Wedding Photography"
}
```

### CreateStyleDto
```json
{
  "styleName": "Portrait Photography"
}
```

### UpdateStyleDto
```json
{
  "styleName": "Updated Style Name"
}
```

### PhotographerStyleResponseDto
```json
{
  "userId": 1,
  "userName": "john_doe",
  "selectedStyles": [
    {
      "styleId": 1,
      "styleName": "Wedding Photography"
    }
  ]
}
```

### StyleSelectionDto
```json
{
  "styleId": 1,
  "styleName": "Wedding Photography",
  "isSelected": true
}
```

### PhotographerStyleDto
```json
{
  "userId": 1,
  "styleId": 1,
  "styleName": "Wedding Photography",
  "userName": "john_doe"
}
```

## Default Styles
The system comes with 20 pre-defined photography styles:
- Wedding Photography
- Portrait Photography
- Event Photography
- Commercial Photography
- Fashion Photography
- Street Photography
- Landscape Photography
- Architecture Photography
- Food Photography
- Product Photography
- Sports Photography
- Wildlife Photography
- Documentary Photography
- Fine Art Photography
- Newborn Photography
- Family Photography
- Engagement Photography
- Corporate Photography
- Real Estate Photography
- Travel Photography

## Usage Examples

### For Photographers
1. **Get available styles for selection**:
   ```
   GET /api/photographerstyles/photographer/{userId}/selection
   ```

2. **Update selected styles**:
   ```
   PUT /api/photographerstyles/photographer/{userId}/styles
   Body: [1, 3, 5, 7]
   ```

### For Clients
1. **Find photographers by style**:
   ```
   GET /api/photographerstyles/style/{styleId}/photographers
   ```

2. **Get photographer's styles**:
   ```
   GET /api/photographerstyles/photographer/{userId}
   ```

### For Administrators
1. **Seed default styles**:
   ```
   POST /api/admin/seed-default-styles
   ```

2. **Create new style**:
   ```
   POST /api/styles
   Body: { "styleName": "New Style" }
   ```

## Error Responses
All endpoints return consistent error responses:
```json
{
  "error": "Error type",
  "message": "Human readable error message",
  "details": "Additional error details (optional)"
}
```

## Authentication
- **Public endpoints**: No authentication required
- **Authenticated endpoints**: Valid JWT token required
- **Admin endpoints**: JWT token with "ADMIN" role required

## Notes
- Photographers can select multiple styles
- Style names must be unique
- All style operations are case-sensitive
- The system prevents duplicate style assignments to the same photographer
