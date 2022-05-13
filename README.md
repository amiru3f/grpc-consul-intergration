# Grpc-Consul intergration

`Grpc-Consul intergration` is a sample dispatcher project in order to intergration Consul discovery with Grpc client factory.

## How to run

Make sure you have Docker installed. Then change your current directory to the root of the project and run 

```console
docker-compose up -d
```
Then you can visit localhost:8080 to check registered up and running services.

gRPC contracts is approved by RequestDispatcher and Providers that is generated from [`available.proto`](https://github.com/amiru3f/grpc-consul-intergration/blob/main/Shared/available.proto) file.