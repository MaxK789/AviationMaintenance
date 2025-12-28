# Lab 3 (gRPC) demo

Run gRPC server:
- src/Aviation.Maintenance.Grpc

Run console client:
set env:
- API_KEY=dev-super-secret-key
- GRPC_WORKORDERS_URL=http://localhost:5004

Run:
dotnet run --project clients/Aviation.Client.Console
