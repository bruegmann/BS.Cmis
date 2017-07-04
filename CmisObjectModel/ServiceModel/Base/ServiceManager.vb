'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************
Imports CmisObjectModel.Common
Imports ss = System.ServiceModel
Imports ssd = System.ServiceModel.Description
Imports sss = System.ServiceModel.Security

Namespace CmisObjectModel.ServiceModel.Base
   ''' <summary>
   ''' Opens and closes servicehosts for baseAdresses
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class ServiceManager
      Implements IDisposable

#Region "Constants"
      Private Const ciMaxReceivedMessageSize As Integer = 52428800
#End Region

#Region "IDisposable Support"
      Private _isDisposed As Boolean

      ' IDisposable
      Protected Overridable Sub Dispose(disposing As Boolean)
         If Not _isDisposed Then
            If disposing Then
               SyncLock _syncObject
                  For Each host As ss.ServiceHost In _hosts.Values
                     Try
                        host.Close()
                     Catch
                     End Try
                  Next
                  _hosts.Clear()
               End SyncLock
            End If
         End If
         Me._isDisposed = True
      End Sub

      Public Sub Dispose() Implements IDisposable.Dispose
         Dispose(True)
         GC.SuppressFinalize(Me)
      End Sub
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' Parameters to create a new servicehost
      ''' </summary>
      ''' <remarks></remarks>
      Private Class OpenHostParameter

         Public Sub New(baseAddress As Uri)
            Me.AbsoluteUri = baseAddress.AbsoluteUri
            Me.BaseAddress = baseAddress
            Me.IsHttps = Me.AbsoluteUri.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
            Me.SecurityMode = If(Me.IsHttps, ss.WebHttpSecurityMode.Transport, ss.WebHttpSecurityMode.TransportCredentialOnly)
         End Sub

         Public ReadOnly AbsoluteUri As String
         Public ReadOnly BaseAddress As Uri
         Public ReadOnly IsHttps As Boolean
         Public ReadOnly SecurityMode As ss.WebHttpSecurityMode
         Public Result As Object
         ''' <summary>
         ''' Used for synchronization between calling thread and workerthread
         ''' </summary>
         ''' <remarks></remarks>
         Public ReadOnly WaitHandle As New System.Threading.EventWaitHandle(False, Threading.EventResetMode.ManualReset)
      End Class

      ''' <summary>
      ''' ContentTypeMapper to receive data-parameter as IO.Stream in WebMethods
      ''' </summary>
      ''' <remarks></remarks>
      Private Class WebContentTypeMapper
         Inherits System.ServiceModel.Channels.WebContentTypeMapper

         ''' <summary>
         ''' Returns always WebContentFormat.Raw
         ''' </summary>
         ''' <param name="contentType"></param>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overrides Function GetMessageFormatForContentType(contentType As String) As System.ServiceModel.Channels.WebContentFormat
            Return System.ServiceModel.Channels.WebContentFormat.Raw
         End Function
      End Class
#End Region

      Private _hosts As New Dictionary(Of String, ss.ServiceHost)
      Private _syncObject As New Object

      ''' <summary>
      ''' Terminates the service for specified baseAddresses
      ''' </summary>
      ''' <param name="baseAddresses"></param>
      ''' <remarks></remarks>
      Public Sub Close(ParamArray baseAddresses As Uri())
         If baseAddresses IsNot Nothing Then
            SyncLock _syncObject
               For Each baseAddress As Uri In baseAddresses
                  If baseAddress IsNot Nothing Then
                     Dim absoluteUri As String = baseAddress.AbsoluteUri

                     If Not String.IsNullOrEmpty(absoluteUri) AndAlso _hosts.ContainsKey(absoluteUri) Then
                        Try
                           _hosts(absoluteUri).Close()
                        Catch
                        End Try
                        _hosts.Remove(absoluteUri)
                     End If
                  End If
               Next
            End SyncLock
         End If
      End Sub

      Protected MustOverride Function GetImplementedContractType() As Type
      Protected MustOverride Function GetServiceType() As Type

      ''' <summary>
      ''' Starts selfhosted WCF services for given baseAddresses
      ''' </summary>
      ''' <param name="baseAddresses"></param>
      ''' <remarks></remarks>
      Public Sub Open(ParamArray baseAddresses As Uri())
         If baseAddresses IsNot Nothing Then
            Try
               'warmup/selftest to determine if an ICmisServiceImpl-instance can be created
               Dim factory As New Generic.ServiceImplFactory(Of Contracts.ICmisServicesImpl)(Nothing)

               SyncLock _syncObject
                  For Each baseAddress As Uri In baseAddresses
                     If baseAddress IsNot Nothing Then
                        Dim params As New OpenHostParameter(baseAddress)
                        Dim thread As New System.Threading.Thread(AddressOf OpenHost)

                        'ensure that there is no host opened for this baseAddress
                        Close(baseAddress)
                        'open servicehost
                        thread.Start(params)
                        'wait max. 20sec for servicehost-start
                        If params.WaitHandle.WaitOne(20000) Then
                           If TypeOf params.Result Is ss.ServiceHost Then
                              _hosts.Add(params.AbsoluteUri, CType(params.Result, ss.ServiceHost))
                           ElseIf TypeOf params.Result Is Exception Then
                              Throw CType(params.Result, Exception)
                           End If
                        Else
                           'don't wait any longer for servicehost
                           System.Threading.ThreadPool.QueueUserWorkItem(Sub(state)
                                                                            thread.Abort()
                                                                         End Sub)
                        End If
                     End If
                  Next
               End SyncLock
            Catch
            End Try
         End If
      End Sub

      ''' <summary>
      ''' Creates a new servicehost in a workerthread to avoid application crashing
      ''' if the service for one baseAddress crashes
      ''' </summary>
      ''' <param name="state"></param>
      ''' <remarks></remarks>
      Private Sub OpenHost(state As Object)
         Dim param As OpenHostParameter = CType(state, OpenHostParameter)

         Try
            Dim host As New ss.ServiceHost(GetServiceType(), param.BaseAddress)
            Dim binding As New ss.WebHttpBinding(param.SecurityMode) With {
               .TransferMode = System.ServiceModel.TransferMode.Streamed,
               .AllowCookies = True,
               .MaxReceivedMessageSize = ciMaxReceivedMessageSize}
            If SupportsClientCredentialType() Then binding.Security.Transport.ClientCredentialType = AppSettings.ClientCredentialType

            Dim endPoint As ssd.ServiceEndpoint = host.AddServiceEndpoint(GetImplementedContractType(), binding, "")
            endPoint.Behaviors.Add(New ssd.WebHttpBehavior With {.HelpEnabled = True})
            binding.ContentTypeMapper = New WebContentTypeMapper()

            Dim debugBehavior As ssd.ServiceDebugBehavior = host.Description.Behaviors.Find(Of ssd.ServiceDebugBehavior)()
            If debugBehavior Is Nothing Then
               debugBehavior = New ssd.ServiceDebugBehavior
               host.Description.Behaviors.Add(debugBehavior)
            End If
            debugBehavior.IncludeExceptionDetailInFaults = AppSettings.SupportsDebugInformation
            If param.IsHttps Then
               debugBehavior.HttpHelpPageEnabled = False
               debugBehavior.HttpsHelpPageUrl = param.BaseAddress.Combine(CmisObjectModel.Constants.ServiceURIs.DebugHelpPageUri)
            Else
               debugBehavior.HttpsHelpPageEnabled = False
               debugBehavior.HttpHelpPageUrl = param.BaseAddress.Combine(CmisObjectModel.Constants.ServiceURIs.DebugHelpPageUri)
            End If

            Dim metaDataBehaviour As ssd.ServiceMetadataBehavior = host.Description.Behaviors.Find(Of ssd.ServiceMetadataBehavior)()
            If metaDataBehaviour IsNot Nothing Then
               If param.IsHttps Then
                  metaDataBehaviour.HttpGetEnabled = False
                  metaDataBehaviour.HttpsGetUrl = param.BaseAddress.Combine(CmisObjectModel.Constants.ServiceURIs.MetaDataUri)
               Else
                  metaDataBehaviour.HttpsGetEnabled = False
                  metaDataBehaviour.HttpGetUrl = param.BaseAddress.Combine(CmisObjectModel.Constants.ServiceURIs.MetaDataUri)
               End If
            End If

            host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = New UserNamePasswordValidator(param.BaseAddress)
            host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = sss.UserNamePasswordValidationMode.Custom
            host.Open()
            param.Result = host
            param.WaitHandle.Set()
         Catch ex As Exception
            param.Result = ex
         End Try
      End Sub

      Protected MustOverride Function SupportsClientCredentialType() As Boolean


   End Class
End Namespace