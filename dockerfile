FROM microsoft/dotnet:latest 
# Set environment variables 
ENV ASPNETCORE_URLS="http://*:5000" 
ENV ASPNETCORE_ENVIRONMENT="Development" 
ENV AWS_ACCESS_KEY_ID="AKIAIX4MJA5ZZS7SBZWA"
ENV AWS_SECRET_ACCESS_KEY=""

# Set working directory 
WORKDIR /app 
# Copy files to app directory 
COPY /my_netcore_application/ ./
WORKDIR /my_netcore_application
# Restore NuGet packages 
RUN ["dotnet", "restore"] 
# Build the app 
RUN ["dotnet", "build"]
# Open port
EXPOSE 5000/tcp 
# Run the app 
ENTRYPOINT ["dotnet", "run"]
