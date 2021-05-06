# Antiques-Auction-WebApp
This project is a web auction application for an antique items seller. 
It allows users to bid on antique items displayed on the web site and admin users to set up items for auction. 
The project only features product management and auctioning, however shopping cart and payment integration do no fall within the scope of this application.

## Requirements
The web app was developed using ASP.NET MVC Core 3.1 and C# 8. Therefore it is necessary to have .NET Core 3.1 SDK installed in order to run the project. If not installed already on your machine, please visit https://dotnet.microsoft.com/download and choose the appropriate installation for your operating system.

## Setup and Usage
1. Once the SDK is installed, and after cloning the project repository, launch your terminal or bash at the project directory. 
2. Change the directory to *Antiques-Auction-WebApp* subfolder.
3. Run the following command in order to build and run the project:

```bash
dotnet run
```
  - The process might take several minutes depending on your machine.
  - Once it is done, you will see the following lines in your command line:
```bash
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
```
  - You can use any of these URLs to open the web application.
4. But beforehand, please extract the zip file containing the images into a folder that should be called *AntiqueItemImages*.  
5. Place that folder inside the *wwwroot* folder in the project directory.
  - Beware any mistake done in the past two steps will not allow images to load onto the website.
6. Now that everything is set up, open the links that any of the two URLs we mentioned previously. 
When prompted to log in, there is a panel for quick login access instead of having to complete the fields. 
However, for future reference, if need be, the following are the credentials to be used:
- **usernames**: admin1, admin2, user1, user2
- They all share the same password: password
 
