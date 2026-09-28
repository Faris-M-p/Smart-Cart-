# Admin Reports Module

## Description
The Reports Module provides business insights based on existing PostgreSQL data using EF Core LINQ aggregations.

## Sub-Modules

### 1. Sales Report
- **View:** GET /Admin/Report/Sales
- **API:** POST /Admin/Report/GetSalesReport
- **Data:** Total Orders, Total Sales, Payment Method summary, Order Status summary, Daily sales summary.
- **Filters:** Date From, Date To.
- **Permission:** Sales.View

### 2. Order Report
- **View:** GET /Admin/Report/Order
- **API:** POST /Admin/Report/GetOrderReport
- **Data:** Order Number, Customer, Date, Total Amount, Method, Payment Status, Order Status.
- **Filters:** Date, Order Status, Payment Status, Payment Method, Search.
- **Permission:** Orders.View

### 3. Product Report
- **View:** GET /Admin/Report/Product
- **API:** POST /Admin/Report/GetProductReport
- **Data:** Product Name, Category Name, Quantity Sold, Sales Amount, Current Stock.
- **Filters:** Search.
- **Permission:** Products.View

### 4. Customer Report
- **View:** GET /Admin/Report/Customer
- **API:** POST /Admin/Report/GetCustomerReport
- **Data:** Customer Name, Email, Total Orders, Total Purchase Amount, Last Order Date.
- **Filters:** Search.
- **Permission:** Employees.View

## Database Access
This module aggregates data directly via EcommerceDbContext (EF Core) on existing tables: Orders, OrderItems, Products, Users, Payments, Stock. It follows the standard Admin architecture returning ApiResponse<T> wrappers.
