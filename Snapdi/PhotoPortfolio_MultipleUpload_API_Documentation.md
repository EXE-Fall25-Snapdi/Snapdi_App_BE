# Multiple Photo Portfolio Upload API Documentation

## Overview
This document describes the new API endpoints for uploading multiple photo URLs at once for photo portfolios.

## New API Endpoints

### 1. Create Multiple Photo Portfolios for Current User
**Endpoint:** `POST /api/PhotoPortfolio/multiple`
**Authorization:** Required (Bearer Token)
**Description:** Create multiple photo portfolios for the currently authenticated user

#### Request Body
```json
{
  "photoUrls": [
    "https://example.com/photo1.jpg",
    "https://example.com/photo2.jpg",
    "https://example.com/photo3.jpg"
  ]
}
```

#### Response (Success - 201 Created)
```json
{
  "createdPortfolios": [
    {
      "photoPortfolioId": 1,
      "userId": 123,
      "photoUrl": "https://example.com/photo1.jpg"
    },
    {
      "photoPortfolioId": 2,
      "userId": 123,
      "photoUrl": "https://example.com/photo2.jpg"
    },
    {
      "photoPortfolioId": 3,
      "userId": 123,
      "photoUrl": "https://example.com/photo3.jpg"
    }
  ],
  "failedPhotoUrls": [],
  "totalAttempted": 3,
  "successCount": 3,
  "failedCount": 0,
  "isCompleteSuccess": true,
  "message": "Successfully created 3 photo portfolios."
}
```

#### Response (Partial Success - 207 Multi-Status)
```json
{
  "createdPortfolios": [
    {
      "photoPortfolioId": 1,
      "userId": 123,
      "photoUrl": "https://example.com/photo1.jpg"
    }
  ],
  "failedPhotoUrls": [
    "invalid-url",
    ""
  ],
  "totalAttempted": 3,
  "successCount": 1,
  "failedCount": 2,
  "isCompleteSuccess": false,
  "message": "Created 1 photo portfolios successfully. 2 failed to create."
}
```

### 2. Create Multiple Photo Portfolios for Specific User (Admin Only)
**Endpoint:** `POST /api/PhotoPortfolio/user/{userId}/multiple`
**Authorization:** Required (Admin Role)
**Description:** Create multiple photo portfolios for a specific user (Admin only)

#### Request Body
```json
{
  "photoUrls": [
    "https://example.com/photo1.jpg",
    "https://example.com/photo2.jpg"
  ]
}
```

#### Response
Same format as the first endpoint.

## Features

### Data Validation
- Automatically filters out null, empty, or whitespace-only URLs
- Removes duplicate URLs
- Validates that at least one valid URL is provided

### Error Handling
- Returns appropriate HTTP status codes:
  - `201 Created`: All photos created successfully
  - `207 Multi-Status`: Some photos created, some failed
  - `400 Bad Request`: No valid URLs provided or user doesn't exist
  - `401 Unauthorized`: Invalid or missing authentication token
  - `500 Internal Server Error`: Unexpected server error

### Bulk Operations
- Uses efficient bulk insert operations when possible
- Falls back to individual inserts if bulk operation fails
- Provides detailed feedback on success and failure counts

## Example Usage

### cURL Example
```bash
curl -X POST "https://yourapi.com/api/PhotoPortfolio/multiple" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "photoUrls": [
      "https://example.com/photo1.jpg",
      "https://example.com/photo2.jpg",
      "https://example.com/photo3.jpg"
    ]
  }'
```

### JavaScript Example
```javascript
const response = await fetch('/api/PhotoPortfolio/multiple', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    photoUrls: [
      'https://example.com/photo1.jpg',
      'https://example.com/photo2.jpg',
      'https://example.com/photo3.jpg'
    ]
  })
});

const result = await response.json();
console.log(`Created ${result.successCount} portfolios successfully`);
```

## Comparison with Single Upload

### Single Upload (Existing)
```json
POST /api/PhotoPortfolio
{
  "photoUrl": "https://example.com/photo1.jpg"
}
```

### Multiple Upload (New)
```json
POST /api/PhotoPortfolio/multiple
{
  "photoUrls": [
    "https://example.com/photo1.jpg",
    "https://example.com/photo2.jpg",
    "https://example.com/photo3.jpg"
  ]
}
```

## Benefits
1. **Efficiency**: Upload multiple photos in a single API call
2. **Performance**: Bulk database operations reduce server load
3. **User Experience**: Faster batch uploads
4. **Reliability**: Detailed success/failure reporting
5. **Flexibility**: Works for both current user and admin operations