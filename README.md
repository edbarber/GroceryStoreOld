# GroceryStore

To use this project you need to have the database created by executing CreateGroceryStoreDb. Make sure the connection string for GroceryStoreConnection in appsettings.json matches the database that was just created locally. 

The models for the GroceryStore database are generated using the scaffolding command referenced here 
https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell

Make sure to delete the OnConfiguring method inside the Models/GroceryStoreContext.cs file whenever your run the scaffolding command as the connection string is stored in appsettings.json.

Use the following commands if you make changes to either ApplicationRole.cs or ApplicationUser.cs (make sure to delete the corresponding files in Data/Migrations and drop the account database beforehand):

Add-Migration ExtendedUserRole -Context ApplicationDbContext

Update-Database -Context ApplicationDbContext

Since the database that holds user accounts is seperate from the grocery store database, the server will check if it exists and create it if it doesn't. If also the accounts database schema is changed then you may have to drop the account database and let the server recreate it.
