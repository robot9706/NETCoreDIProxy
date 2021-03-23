# .NET Core DI proxy

This example shows how to use a `DispatchProxy` to proxy an implementation of a service to an interface. The implementation class does not actually implement the service interface, it only has the same methods with the same arguments.

The proxy service acts as an actual implementation and proxies the method calls.

This can be used to give an implementation to a service where it is not possible to use the service interface for the implementation.