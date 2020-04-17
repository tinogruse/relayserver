# Glossary

When you are new to **RelayServer** or are coming from an older version, there can be a lot of words to learn. This glossary aims to give you a 25.000-feet overview of common terms and what they mean in the context of RelayServer.

[C](#c) | [I](#i) | [R](#r) | [T](#t)

## C

### Client

A *Client* is an external application or a service, which is sending [Request](#request) to a [Target](#target) which is made accessible through the [RelayServer](#relayserver).

### Connector

The RelayServer *Connector* is a piece of software that runs on a physical location where you want to access local services (aka [Targets](#target)). The network the *Connector* is located in is usually not accessible from the internet. The *Connector* creates a connection to the [RelayServer](#relayserver), through which the server can send a [Request](#request) to the connector. The connector then requests the internal [Target](#target) and relays its [Response](#response) back to the server, which then relays it back to the requesting [Client](#client).

For reasons of availability and load balacing the *Connector* can be run multiple times at the same location / network. All *Connectors* on a specific physical location are logically referred to as a [Tenant](#tenant).

The *Connector* was formerly called *OnPremisesConnector* (short *OPC*) in RelayServer v2.

## R

### RelayServer

The *RelayServer* is a service that usually is publicly available on the internet. Its main purpose is to receive [Requests](#request) from [Clients](#client), and pass them to a [Connector](#connector) that belongs to the correct [Tenant](#tenant). It then waits for the [Response](#response) to be sent back, and passes it back to the [Client](#client). This process is referred to as *Relaying*.

### Request

The *Request* represents an external Http(s) request from a [Client](#client). It can be intercepted and modified while being processed by the [RelayServer](#relayserver). It will be passed on to a [Target](#target) via the [Connector](#connector).

### Response

A *Response* always corresponds to a [Request](#request). When the [Request](#request) was executed by the [Target](#target), the [Connector](#connector) will read the *Response* and send it back to the [RelayServer](#relayserver).

## I

### Interceptor

An *Interceptor* is a piece of code that you can provide via dependency injection and that is able to intercept

* a [Requests](#request) after it was received by the [RelayServer](#relayserver) and before it is passed along to the [Connector](#connector) or
* a [Response](#response) after it was received from the [Connector](#connector) and before it is passed back to the [Clients](#client).

*Interceptors* are a flexible way of extending the [RelayServer](#relayserver) functionality and can be used to modify the corresponding [Requests](#request) or [Responses](#response) by changing url, method, http headers, the body (payload).

## T

### Target

A *Target* describes a service that is usually not directly exposed to the internet. Instead it is accessible via a [Request](#request) sent to the [RelayServer](#relayserver). This [Request](#request) is then relayed through a [Connector](#connector) into the [Tenants](#tenant) network and then executed. The [Response](#response) of the *Target* is then sent back to the [RelayServer](#relayserver) which will then relay it back to the [Client](#client).

### Tenant

The *Tenant* describes a physical location (on-premises) where one or more [Connectors](#connector) are installed and ready to relay requests to local [Targets](#target) that are provided by the *Tenant*.

The *Tenant* was formerly called *Link* in RelayServer v2.