# Another Azure Functions and Cosmos DB Graph API blog

As soon as our partners in the Cosmos DB team released the [Graph API](https://docs.microsoft.com/en-us/azure/cosmos-db/graph-introduction) earlier this year I knew I would have to build a demo leveraging [Azure Functions](https://azure.microsoft.com/en-us/services/functions/) and [Azure Functions Proxies](https://docs.microsoft.com/en-us/azure/azure-functions/functions-proxies) to interact with a graph data model. 

**This blog includes a sample that creates a generic REST API on top of Cosmos DB Graph API as well as a sample Web page to interact with it.**

## What you would need to make the solution work
To get this sample working you would need: 
* [An Azure Subscription](https://azure.microsoft.com/en-us/free/)
* [Git client](https://git-scm.com/) ([GitHub](https://github.com/) account is recommended)
* [Visual Studio 2017 Tools for Azure Functions](https://blogs.msdn.microsoft.com/webdev/2017/05/10/azure-function-tools-for-visual-studio-2017/) installed

## Why is your demo different than other blog posts?

Great question! The Graph API uses [Gremlin](https://github.com/tinkerpop/gremlin/wiki) syntax to interact with a Graph. I figured it would be nice if we can easily write a REST API that performs simple CRUD operations against a Graph. 

## Problem we are trying to solve
Consider a simple Gremlin query to add a new Vertex to a graph. You would need to connect to your Cosmos DB and send it a string that looks like: 

```javascript
g.addV('Vertex Name').property('id', 'Vertex ID')
```

What you would want is a REST API that looks like: 

```javascript
https://MyRESTAPI/api/{Cosmons DB Collection}/vertex/add/{Vertex ID}/{Vertex Name}
```
So this is really about constructing a REST API path, mapping the pieces of the URL path to a Gremlin expression and sending this expression to Cosmos DB.

## Creating the REST API

Since coding is not really my core competency I was looking for a way to have as little code as possible and be able to add APIs using a declarative format. This is where Azure Functions Proxies came to the rescue! With Proxies we can map many URL path to on Azure Function, the Function can then investigate the incoming data and assemble the Gremlin expression.

The sample app includes:
1. A Cosmos DB database 
2. An Azure Function
3. A Proxies json file
4. A query map json file used to generate the Gremlin queries
5. An HTML\ Java Script file used as a test client

# Setting up the sample
1. Create a new Cosmos DB database (you can use an existing one if you want). To create a new Database:
 - Follow the steps in [this link](https://docs.microsoft.com/en-us/azure/cosmos-db/create-graph-gremlin-console#create-a-database-account) to create a new Database. No need to follow the other steps in the doc. 
  **Note: Graphs will be automatically created if they don't existing within the database. The term Graph and Collection interchangeable in this blog**

2. Copy the database name, URI and one of the keys in the Keys screen of your Cosmos DB Database (click the Keys menu item on the left for the URI and key values). You will use them later.

3. Create a new [Azure Functions App](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-function-app-portal#create-a-function-app).

4. Go to the Platform features tab and create 3 new App Settings in your new 
Function App by following the steps [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#platform-features-tab) and [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#settings):
  - Endpoint
  - AuthKey
  - databaseName

5. Fill in the values you copied in step 2 into your Function App Settings:
  - Endpoint will be the Cosmos DB URI
  - AuthKey will be the Cosmos DB key
  - databaseName will be the Cosmos DB Graph name

6. [Enable Azure Functions Proxies (currently in preview)](https://docs.microsoft.com/en-us/azure/azure-functions/functions-proxies#enable)

7. Clone the sample GitHub repository locally 
```javascript
git clone https://github.com/nirmash/CosmosDBGraphREST.git
```
and browse to the code folder
```javascript
cd CosmosDBGraphREST\CosmosDBGraphREST
```
8. Open GraphFunc.sln (should be in the CosmosDBGraphREST folder) in Visual Studio 2017.

9. Right-click on the GraphFunc solution and select Publish. Click the "Select Existing" radio button and hit the Publish button.  

10. Find the Functions App you created in step 3

11. Browse to your test page
```javascript
https://{Function App Name}/GraphUI
```
Your REST API should now be up and running!

# Loading some data into your graph
Use the curl commands below or just paste the Urls in a browser to load a bit of data into your graph. In this case, a car brand and car types belonging to this brand. A graph named "cars" will be created in your Comsmos DB database. 
```javascript
curl https://{Function App Name}/cars/addvertex/Audi/Brand
curl https://{Function App Name}/cars/addvertex/A4/Sedan
curl https://{Function App Name}/cars/addvertex/A6/Sedan
curl https://{Function App Name}/cars/addvertex/Q5/SUV
curl https://{Function App Name}/cars/addvertex/Q7/SUV
curl https://{Function App Name}/cars/addedge/Audi/A4/Sedans
curl https://{Function App Name}/cars/addedge/Audi/A6/Sedans
curl https://{Function App Name}/cars/addedge/Audi/Q5/SUVs
curl https://{Function App Name}/cars/addedge/Audi/Q7/SUVs
```
Now to run a couple of queries against your new graph! 
To see all the items (Vertex) in the graph, type the below in a browser:
```javascript
curl https://{Function App Name}/cars/selectcollection
```
To see all the cars in the graph that are defined as Sedans:
```javascript
https://{Function App Name}/cars/selectedgeout/Sedans
```
You will see a JSON string coming back like the below:
```javascript
[
  {
    "id": "A4",
    "label": "Sedan",
    "type": "vertex"
  },
  {
    "id": "A6",
    "label": "Sedan",
    "type": "vertex"
  }
]
```
# What's next? 
This blog suggested a way to take the generic Gremlin query language and use an Azure Function and Azure Functions Proxies to create a REST API that performs CRUD actions against Cosmos DB Graph API collections. 
There is plenty of information out there about Azure Functions, Azure Functions Proxies and Cosmos DB. Leave me comments or questions and play with the code! 
