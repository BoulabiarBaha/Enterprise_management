RUN PROJECT: dotnet run

WORKFLOW:
1- Create a Client
2- Create a Product
3- Create a Transaction (manually using Post endpoint) => Create a Billing and add "BillingId" in Transaction => Update "TransactionIds" , "BillingIds" and "Value" in Client 
HOW TO RUN THE PROJECT:
0- Open Git bash (Run as administrator) and tap: net start mongoDB
1- Build Myapp.sln
2- dotnet run
3- Navigate to "http://localhost:port/swagger"
