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
Imports System.Net
Imports System.ServiceModel

Namespace CmisObjectModel.Common
   ''' <summary>
   ''' Common collection of generic method proxies which allow the call of generic methods at runtime, if only 
   ''' information about the RuntimeType but not the generic type argument itself can be provided.
   ''' </summary>
   ''' <remarks>
   ''' Sample.
   ''' Assuming the module MyModule contains the generic method:
   '''    Public Function MyFunc(Of T)(argumentList) As ReturnType
   ''' 
   ''' If T is known at compiletime (for example: String) then the call is easy:
   '''    MyModule.MyFunc(Of String)(argumentList)
   ''' 
   ''' But what is to do, if the type isn't known at compiletime but at runtime as GetType(T)? In this case widen the
   ''' GenericRuntimeHelper:
   '''    Public MustOverride Function Call_MyModule_MyFunc(argumentList) As ReturnType
   ''' 
   ''' and override this method in the nested GenericTypeHelper(Of T) class:
   '''    Public Overrides Function Call_MyModule_MyFunc(argumentList) As ReturnType
   '''       Return MyModule.MyFunc(Of T)(argumentList)
   '''    End Function
   ''' 
   ''' Now You can call MyModule.MyFunc(Of String)() at runtime by:
   '''    CmisObjectModelLibrary.Common.GenericRuntimeHelper.GetInstance(GetType(String)).CallMyModule_MyFunc(argumentList)
   ''' </remarks>
   Public MustInherit Class GenericRuntimeHelper

#Region "Constructors"
      ''' <summary>
      ''' Creation only possible from nested classes
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub New()
      End Sub

      Private Shared _runtimeHelpers As New Dictionary(Of Type, GenericRuntimeHelper)
      ''' <summary>
      ''' Creates a GenericTypeHelper-instance for specified type
      ''' </summary>
      ''' <param name="type"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GetInstance(type As Type) As GenericRuntimeHelper
         If type Is Nothing Then
            Return Nothing
         Else
            SyncLock _runtimeHelpers
               If _runtimeHelpers.ContainsKey(type) Then
                  Return _runtimeHelpers(type)
               ElseIf GetType(System.Enum).IsAssignableFrom(type) Then
                  'create a GenericTypeHelper-instance for enums using reflection
                  Dim genericTypeDefinitionType As Type = GetType(GenericEnumTypeHelper(Of enumClientType)).GetGenericTypeDefinition
                  Dim genericType As Type = genericTypeDefinitionType.MakeGenericType(type)
                  Dim ci As System.Reflection.ConstructorInfo = genericType.GetConstructor(New Type() {})
                  Dim retVal As GenericRuntimeHelper = CType(ci.Invoke(New Object() {}), GenericRuntimeHelper)

                  _runtimeHelpers.Add(type, retVal)
                  Return retVal
               ElseIf type.IsValueType Then
                  'create a GenericTypeHelper-instance for value type using reflection
                  Dim genericTypeDefinitionType As Type = GetType(GenericValueTypeHelper(Of Boolean)).GetGenericTypeDefinition
                  Dim genericType As Type = genericTypeDefinitionType.MakeGenericType(type)
                  Dim ci As System.Reflection.ConstructorInfo = genericType.GetConstructor(New Type() {})
                  Dim retVal As GenericRuntimeHelper = CType(ci.Invoke(New Object() {}), GenericRuntimeHelper)

                  _runtimeHelpers.Add(type, retVal)
                  Return retVal
               Else
                  'create an unspecific GenericTypeHelper-instance using reflection
                  Dim genericTypeDefinitionType As Type = GetType(GenericTypeHelper(Of Object)).GetGenericTypeDefinition
                  Dim genericType As Type = genericTypeDefinitionType.MakeGenericType(type)
                  Dim ci As System.Reflection.ConstructorInfo = genericType.GetConstructor(New Type() {})
                  Dim retVal As GenericRuntimeHelper = CType(ci.Invoke(New Object() {}), GenericRuntimeHelper)

                  _runtimeHelpers.Add(type, retVal)
                  Return retVal
               End If
            End SyncLock
         End If
      End Function
#End Region

#Region "Helper classes"
      ''' <summary>
      ''' GenericTypeHelper only for enums
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <remarks></remarks>
      Private Class GenericEnumTypeHelper(Of T As Structure)
         Inherits GenericValueTypeHelper(Of T)

         Public Overrides Function Convert(value As Object) As String
            Return CType(value, System.Enum).GetName()
         End Function

         Public Overrides Function ConvertBack(value As String, defaultValue As Object) As Object
            Dim result As T
            Return If(value.TryParse(result, True), result, defaultValue)
         End Function

         ''' <summary>
         ''' TryParse() for type T (enum type)
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overrides Function TryParseGeneric(name As String, ByRef genericValue As System.Enum, Optional ignoreCase As Boolean = False) As Boolean
            Dim value As T = CType(CType(genericValue, Object), T)

            If name.TryParse(value, ignoreCase) Then
               genericValue = CType(CType(value, Object), System.Enum)
               Return True
            End If

            Return False
         End Function
      End Class

      ''' <summary>
      ''' GenericTypeHelper for valueType
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <remarks></remarks>
      Private Class GenericValueTypeHelper(Of T As Structure)
         Inherits GenericTypeHelper(Of T)

         Public Overrides Function GetValue(nullable As Object) As Object
            If TypeOf nullable Is T? Then
               With CType(nullable, T?)
                  If .HasValue Then
                     Return .Value
                  End If
               End With
            End If

            'failure
            Return Nothing
         End Function

         Public Overrides Function HasValue(nullable As Object) As Boolean
            Return TypeOf nullable Is T? AndAlso CType(nullable, T?).HasValue
         End Function
      End Class

      ''' <summary>
      ''' Unspecific GenericTypeHelper
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <remarks></remarks>
      Private Class GenericTypeHelper(Of T)
         Inherits GenericRuntimeHelper

         ''' <summary>
         ''' Common.Convert(Of T)()
         ''' </summary>
         Public Overrides Function Convert(value As Object) As String
            Return Common.Convert(Of T)(CType(value, T))
         End Function

         ''' <summary>
         ''' Common.ConvertBack(Of T)()
         ''' </summary>
         Public Overrides Function ConvertBack(value As String, defaultValue As Object) As Object
            Return Common.ConvertBack(Of T)(value, CType(defaultValue, T))
         End Function

         ''' <summary>
         ''' Converts dictionary to IDictionary(Of TKey, TValue)
         ''' </summary>
         Public Overrides Function ConvertDictionary(Of TKey, TValue)(dictionary As Object) As System.Collections.Generic.IDictionary(Of TKey, TValue)
            If dictionary Is Nothing Then
               Return Nothing
            ElseIf TypeOf dictionary Is IDictionary(Of TKey, TValue) Then
               Return CType(dictionary, IDictionary(Of TKey, TValue))
            Else
               Dim dictionaryType As Type = GetType(IDictionary(Of TKey, TValue))
               Dim genericIDictionaryType As Type = dictionaryType.GetGenericTypeDefinition()

               'search for IDictionary(Of)
               For Each interfaceType As Type In dictionary.GetType().GetInterfaces()
                  If interfaceType.IsGenericType AndAlso interfaceType.GetGenericTypeDefinition() Is genericIDictionaryType Then
                     Dim genericArguments As Type() = interfaceType.GetGenericArguments()
                     Dim keyType As Type = genericArguments(0)
                     Dim valueType As Type = genericArguments(1)

                     'this instance can be used to define TSourceKey
                     If GetType(T) Is keyType Then
                        Return ConvertDictionaryCore(Of TKey, TValue, T)(valueType, dictionary)
                     Else
                        Return GenericRuntimeHelper.GetInstance(keyType).ConvertDictionaryCore(Of TKey, TValue)(valueType, dictionary)
                     End If
                  End If
               Next

               If TypeOf dictionary Is IDictionary Then
                  'IDictionary
                  Dim retVal As New Dictionary(Of TKey, TValue)

                  For Each de As DictionaryEntry In CType(dictionary, IDictionary)
                     If TypeOf de.Key Is TKey AndAlso (de.Value Is Nothing OrElse TypeOf de.Value Is TValue) Then
                        retVal.Add(CType(de.Key, TKey), CType(de.Value, TValue))
                     End If
                  Next
                  Return retVal
               Else
                  'no IDictionary interface found
                  Return Nothing
               End If
            End If
         End Function

         Protected Overrides Function ConvertDictionaryCore(Of TKey, TValue)(
            sourceValueType As System.Type, dictionary As Object) As IDictionary(Of TKey, TValue)
            Return ConvertDictionaryCore(Of TKey, TValue, T)(sourceValueType, dictionary)
         End Function
         Protected Overrides Function ConvertDictionaryCore(Of TKey, TValue, TSourceKey)(dictionary As Object) As IDictionary(Of TKey, TValue)
            Dim retVal As New Dictionary(Of TKey, TValue)

            For Each de As KeyValuePair(Of TSourceKey, T) In CType(dictionary, IDictionary(Of TSourceKey, T))
               If TypeOf de.Key Is TKey AndAlso (de.Value Is Nothing OrElse TypeOf de.Value Is TValue) Then
                  retVal.Add(CType(CObj(de.Key), TKey), CType(CObj(de.Value), TValue))
               End If
            Next
            Return retVal
         End Function
         Private Overloads Function ConvertDictionaryCore(Of TKey, TValue, TSourceKey)(sourceValueType As System.Type, dictionary As Object) As IDictionary(Of TKey, TValue)
            Return GenericRuntimeHelper.GetInstance(sourceValueType).ConvertDictionaryCore(Of TKey, TValue, T)(dictionary)
         End Function

         ''' <summary>
         ''' Creates a cmisFaultType to encapsulate an exception
         ''' </summary>
         Public Overrides Function CreateCmisFaultType(ex As System.Exception) As Messaging.cmisFaultType
            If TypeOf ex Is System.ServiceModel.Web.WebFaultException(Of T) Then
               Return Messaging.cmisFaultType.CreateInstance(Of T)(CType(ex, System.ServiceModel.Web.WebFaultException(Of T)))
            ElseIf TypeOf ex Is System.ServiceModel.Web.WebFaultException Then
               Return Messaging.cmisFaultType.CreateInstance(CType(ex, System.ServiceModel.Web.WebFaultException))
            Else
               Return Messaging.cmisFaultType.CreateInstance(ex)
            End If
         End Function

         ''' <summary>
         ''' Returns the count property of ICollection or ICollection(Of) instances
         ''' </summary>
         Public Overrides Function GetCount(collection As Object) As Integer
            If collection Is Nothing Then
               Return 0
            ElseIf TypeOf collection Is ICollection Then
               Return CType(collection, ICollection).Count
            ElseIf TypeOf collection Is ICollection(Of T) Then
               Return CType(collection, ICollection(Of T)).Count
            Else
               Dim collectionType As Type = GetType(ICollection(Of T))
               Dim genericICollectionType As Type = collectionType.GetGenericTypeDefinition()
               For Each interfaceType As Type In collection.GetType().GetInterfaces()
                  If interfaceType.IsGenericType AndAlso interfaceType.GetGenericTypeDefinition() Is genericICollectionType Then
                     Return GenericRuntimeHelper.GetInstance(interfaceType.GetGenericArguments()(0)).GetCount(collection)
                  End If
               Next

               'no ICollection interface found
               Return 0
            End If
         End Function

         ''' <summary>
         ''' Extracts StatusCode from WebFaultException and generic WebFaultException instances
         ''' </summary>
         ''' <param name="exception"></param>
         ''' <returns></returns>
         Protected Overrides Function GetStatusCodeCore(exception As FaultException) As HttpStatusCode
            If TypeOf exception Is System.ServiceModel.Web.WebFaultException(Of T) Then
               Return DirectCast(exception, System.ServiceModel.Web.WebFaultException(Of T)).StatusCode
            Else
               Return System.Net.HttpStatusCode.InternalServerError
            End If
         End Function

         ''' <summary>
         ''' Object types cannot be designed as nullable
         ''' </summary>
         Public Overrides Function GetValue(nullable As Object) As Object
            Return nullable
         End Function

         ''' <summary>
         ''' Object types cannot be designed as nullable
         ''' </summary>
         Public Overrides Function HasValue(nullable As Object) As Boolean
            Return nullable IsNot Nothing
         End Function

         ''' <summary>
         ''' Returns True if dictionary could be converted to IDictionary(Of TKey, TValue)
         ''' </summary>
         ''' <param name="dictionary">If successful the value is changed to IDictionary(Of TKey, TValue) type</param>
         Public Overrides Function TryConvertDictionary(Of TKey, TValue)(ByRef dictionary As Object) As Boolean
            If dictionary Is Nothing Then
               Return False
            ElseIf TypeOf dictionary Is IDictionary(Of TKey, TValue) Then
               Return True
            Else
               Dim dictionaryType As Type = GetType(IDictionary(Of TKey, TValue))
               Dim genericIDictionaryType As Type = dictionaryType.GetGenericTypeDefinition()

               For Each interfaceType As Type In dictionary.GetType().GetInterfaces()
                  If interfaceType.IsGenericType AndAlso interfaceType.GetGenericTypeDefinition() Is genericIDictionaryType Then
                     Dim genericArguments As Type() = interfaceType.GetGenericArguments()
                     Dim keyType As Type = genericArguments(0)
                     Dim valueType As Type = genericArguments(1)
                     Dim result As IDictionary(Of TKey, TValue) = If(GetType(T) Is keyType,
                                                                     ConvertDictionaryCore(Of TKey, TValue, T)(valueType, dictionary),
                                                                     GenericRuntimeHelper.GetInstance(keyType).ConvertDictionaryCore(Of TKey, TValue)(valueType, dictionary))
                     If result IsNot Nothing AndAlso result.Count = GetCount(dictionary) Then
                        dictionary = result
                        Return True
                     End If
                  End If
               Next
               'expected interface not found
               Return False
            End If
         End Function

         ''' <summary>
         ''' for implementation requirements only
         ''' </summary>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Public Overrides Function TryParseGeneric(name As String, ByRef genericValue As System.Enum, Optional ignoreCase As Boolean = False) As Boolean
            'TryParse() can only be called for enum types; type T IS NOT an enum type (see GenericRuntimeHelper.GetInstance())
            Return False
         End Function
      End Class
#End Region

#Region "Support for generic calls"
      ''' <summary>
      ''' Convert version for unspecific value
      ''' </summary>
      Public MustOverride Function Convert(value As Object) As String

      ''' <summary>
      ''' ConvertBack version for unspecific defaultValue
      ''' </summary>
      Public MustOverride Function ConvertBack(value As String, defaultValue As Object) As Object

      ''' <summary>
      ''' Converts dictionary to IDictionary(Of TKey, TValue)
      ''' </summary>
      Public MustOverride Function ConvertDictionary(Of TKey, TValue)(dictionary As Object) As IDictionary(Of TKey, TValue)

      ''' <summary>
      ''' internal use
      ''' </summary>
      Protected MustOverride Function ConvertDictionaryCore(Of TKey, TValue)(sourceValueType As Type, dictionary As Object) As IDictionary(Of TKey, TValue)
      ''' <summary>
      ''' internal use
      ''' </summary>
      Protected MustOverride Function ConvertDictionaryCore(Of TKey, TValue, TSourceKey)(dictionary As Object) As IDictionary(Of TKey, TValue)

      ''' <summary>
      ''' Creates a cmisFaultType to encapsulate an exception
      ''' </summary>
      Public MustOverride Function CreateCmisFaultType(ex As Exception) As Messaging.cmisFaultType

      ''' <summary>
      ''' Returns the count property of ICollection or ICollection(Of) instances
      ''' </summary>
      Public MustOverride Function GetCount(collection As Object) As Integer

      ''' <summary>
      ''' Returns the HttpStatusCode of WebFaultExceptions
      ''' </summary>
      Public Shared Function GetStatusCode(exception As System.ServiceModel.FaultException) As System.Net.HttpStatusCode
         If exception Is Nothing Then
            Return HttpStatusCode.OK
         ElseIf TypeOf exception Is System.ServiceModel.Web.WebFaultException Then
            'System.ServiceModel.Web.WebFaultException
            Return DirectCast(exception, System.ServiceModel.Web.WebFaultException).StatusCode
         Else
            'System.ServiceModel.Web.WebFaultException(Of T)
            Dim exceptionType As Type = exception.GetType()

            While exceptionType IsNot Nothing
               If exceptionType.IsGenericType AndAlso
                  exceptionType.GetGenericTypeDefinition Is _genericWebFaultExceptionTypeDefinition Then
                  Dim genericArguments As Type() = exceptionType.GetGenericArguments()
                  Return GetInstance(genericArguments(0)).GetStatusCodeCore(exception)
               End If
               exceptionType = exceptionType.BaseType
            End While

            Return HttpStatusCode.InternalServerError
         End If
      End Function
      Protected MustOverride Function GetStatusCodeCore(exception As System.ServiceModel.FaultException) As System.Net.HttpStatusCode

      ''' <summary>
      ''' Returns the value of a nullable
      ''' </summary>
      Public MustOverride Function GetValue(nullable As Object) As Object

      ''' <summary>
      ''' Returns true if nullable has a value
      ''' </summary>
      Public MustOverride Function HasValue(nullable As Object) As Boolean

      ''' <summary>
      ''' Returns True if dictionary could be converted to IDictionary(Of TKey, TValue)
      ''' </summary>
      ''' <param name="dictionary">If successful the value is changed to IDictionary(Of TKey, TValue) type</param>
      Public MustOverride Function TryConvertDictionary(Of TKey, TValue)(ByRef dictionary As Object) As Boolean

      ''' <summary>
      ''' TryParse version for unspecific enum type
      ''' </summary>
      Public MustOverride Function TryParseGeneric(name As String, ByRef genericValue As System.Enum, Optional ignoreCase As Boolean = False) As Boolean
#End Region

      Private Shared _genericWebFaultExceptionTypeDefinition As Type = GetType(System.ServiceModel.Web.WebFaultException(Of String)).GetGenericTypeDefinition()

   End Class
End Namespace