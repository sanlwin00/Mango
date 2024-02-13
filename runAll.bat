start cmd.exe /C "dotnet watch run --project .\Mango.Services.AuthAPI\ --launch-profile https"
start cmd.exe /C "dotnet watch run --project .\Mango.Services.ProductAPI\ --launch-profile https"
start cmd.exe /C "dotnet watch run --project .\Mango.Services.CouponAPI\ --launch-profile https"
start cmd.exe /C "dotnet watch run --project .\Mango.Services.ShoppingCartAPI\ --launch-profile https"
dotnet watch run --project .\Mango.Web\
