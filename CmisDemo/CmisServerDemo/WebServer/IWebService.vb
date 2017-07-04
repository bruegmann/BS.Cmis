
<ServiceModel.ServiceContract(Namespace:="http://demo.bsw/cmis")>
Public Interface IWebService

   <ServiceModel.OperationContract()> <ServiceModel.Web.WebGet(UriTemplate:="obj?id={objectId}")>
   Function ShowObject(objectId As String) As IO.Stream

   <ServiceModel.OperationContract()> <ServiceModel.Web.WebGet(UriTemplate:="file?id={objectId}")>
   Function GetContent(objectId As String) As IO.Stream

   <ServiceModel.OperationContract()> <ServiceModel.Web.WebGet(UriTemplate:="meta?id={objectId}")>
   Function GetMetadata(objectId As String) As IO.Stream

End Interface
