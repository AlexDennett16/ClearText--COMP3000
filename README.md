# ClearText--COMP3000

ClearText is a desktop writing application designed to help users refine their writing throughNLP analysis.   
Unlike many modern writing tools, ClearText does not use generative AI or require an internet connection, prioritising ethical use of AI-assisted technologies.  
This project was developed as part of the COMP3000 Computing Project at the University of Plymouth.  

## Running the Application - In IDE

### Requirements
- .NET SDK 8.X
- Python 3.X
- Python dependencies listed in `requirements.txt` - instructions below


### Setup
1. Clone the repo
2. CD into \ClearText--COMP3000\Backend
3. Install Python dependencies:
   ```bash
   pip install -r requirements.txt
4. CD back to root
5. Run the application with
   ```bash
   Dotnet Run

## Running the Application - End users

ClearText is designed to run locally without additional configuration. Once development is complete it would be compiled into a downloadable exe that users would be able to run with no addiitonal setup



## Architecture

The application is split into 2 distinct entities, with a server medium to connect them 

### C# Frontend

This is a Dotnet Avalonia UI that provides all the frontend capabilitties of the application. All user interaction occurs here

### Python Backend  

This is a Python NLTK based pipeline that accepts a serialized JSON and tokenises it. Then passes through several rules, applying errors and suggestions to change them with, then returning a series of errors, made up of token location, error type and suggestion to fix it   

### gRPC Server Connection

facilitates connection between parent C# and childPython processes. Formats communication into an agreed upon template, and talks over port 50051
