# H1 Another Azure Functions and Cosmos DB Graph API blog

As soon as our partners in the Cosmos DB team released the [Graph API](https://docs.microsoft.com/en-us/azure/cosmos-db/graph-introduction) earlier this year I knew I would have to build a demo leveraging [Azure Functions](https://azure.microsoft.com/en-us/services/functions/) and [Azure Functions Proxies](https://docs.microsoft.com/en-us/azure/azure-functions/functions-proxies) to interact with a graph data model. 

**This blog includes a sample that creates a generic REST API on top of Cosmos DB Graph API as well as a sample Web page to interact with it.**

# H2 What you would need to make the solution work
To get this sample working you would need: 
* [An Azure Subscription](https://azure.microsoft.com/en-us/free/)
* [Git client](https://git-scm.com/) ([GitHub](https://github.com/) account is recommended)
* [Visual Studio 2017 Tools for Azure Functions](https://blogs.msdn.microsoft.com/webdev/2017/05/10/azure-function-tools-for-visual-studio-2017/) installed

# H2 Why is your demo different then other blog posts?

Great question! The Graph API uses [Gremlin](https://github.com/tinkerpop/gremlin/wiki) syntax to interact with a Graph. I figured it would be nice if we can easily write a REST API that performs simple CRUD operations against a Graph. 

# H2 Problem we are trying to solve
Consider a simple Gremlin query to add a new Vertex to a graph. You would need to connect to your Cosmos DB and send it a string that looks like: 

```javascript
g.addV('Vertex Name').property('id', 'Vertex ID')
```

What you would want is a REST API that looks like: 

```javascript
https://MyRESTAPI/api/{Cosmons DB Collection}/vertex/add/{Vertex ID}/{Vertex Name}
```
So this is really about constructing a REST API path, mapping the pieces of the URL path to a Gremlin expression and sending this expression to Cosmos DB.

#H2 Creating the REST API

Since coding is not really my core competency I was looking for a way to have as little code as possible and be able to add APIs using a declarative format. This is where Azure Functions Proxies came to the rescue! With Proxies we can map many URL path to on Azure Function, the Function can then investigate the incoming data and assemble the Gremlin expression.

The sample app includes:
1. A Cosmos DB database 
2. An Azure Function
3. A Proxies json file
4. A query map json file used to generate the Gremlin queries
5. An HTML\ Java Script file used as a test client

#H2 Setting up the sample

