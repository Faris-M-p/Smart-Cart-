# Empty State Handler - Quick Reference

## Global Object
```javascript
emptyState  // Global instance of EmptyStateHandler
```

## Quick Methods

### Show No Data
```javascript
emptyState.showNoData('tableBodyId', options, colspan);
```

### Show Error (Auto-detect)
```javascript
emptyState.showError('tableBodyId', error, options, colspan);
```

### Show Specific State
```javascript
emptyState.showInTable('tableBodyId', 'state-type', options, colspan);
```

## Available States
- `'no-data'` - No results found
- `'no-internet'` - Network connection error
- `'server-error'` - Server error (500)
- `'service-unavailable'` - Service unavailable (503)
- `'maintenance'` - Under maintenance
- `'not-found'` - Page not found (404)
- `'access-denied'` - Access denied (403)
- `'coming-soon'` - Coming soon
- `'session-expired'` - Session expired

## Example: Product Listing
```javascript
// No products found
if (products.length === 0) {
    emptyState.showNoData('productTableBody', {
        title: 'No products found',
        onReset: 'resetFilters()',
        onClear: 'clearSearch()'
    }, 6);
}

// Error occurred
catch (error) {
    emptyState.showError('productTableBody', error, {}, 6);
}
```

## Example: Category Listing
```javascript
// No categories found
if (categories.length === 0) {
    emptyState.showNoData('categoryTableBody', {
        title: 'No categories found'
    }, 5);
}
```

## Custom Buttons
```javascript
emptyState.showInTable('tableBodyId', 'no-data', {
    buttons: [
        { text: 'Add New', class: 'btn-primary', onclick: 'openModal()', icon: 'ti-plus' },
        { text: 'Reset', class: 'btn-secondary', onclick: 'reset()', icon: 'ti-refresh' }
    ]
}, 6);
```
