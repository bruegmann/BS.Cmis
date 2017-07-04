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
Imports ccg = CmisObjectModel.Collections.Generic
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Core.Collections
   <sxs.XmlRoot("properties", Namespace:=Constants.Namespaces.cmis),
    Attributes.JavaScriptConverter(GetType(JSON.Collections.PropertiesConverter))>
   Partial Public Class cmisPropertiesType
      Implements IEnumerable

      Public Sub New(ParamArray properties As Core.Properties.cmisProperty())
         _properties = properties
      End Sub

      Public Shared Widening Operator CType(value As List(Of Core.Properties.cmisProperty)) As cmisPropertiesType
         If value Is Nothing Then
            Return Nothing
         Else
            Return New cmisPropertiesType(value.ToArray())
         End If
      End Operator

      Public Shared Widening Operator CType(value As Core.Properties.cmisProperty) As cmisPropertiesType
         If value Is Nothing Then
            Return Nothing
         Else
            Return New cmisPropertiesType(value)
         End If
      End Operator

      Public Shared Widening Operator CType(value As Core.Properties.cmisProperty()) As cmisPropertiesType
         If value Is Nothing Then
            Return Nothing
         Else
            Return New cmisPropertiesType(value)
         End If
      End Operator

#Region "IEnumerable"
      Public Function GetEnumerator() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
         Return If(_properties, New Core.Properties.cmisProperty() {}).GetEnumerator()
      End Function
#End Region

#Region "IXmlSerialization"
      Public Overrides Sub ReadXml(reader As System.Xml.XmlReader)
         MyBase.ReadXml(reader)

         'support: foreach cmisProperty.ExtendedProperty(DeclaringType)
         If _properties IsNot Nothing Then
            Dim objectTypeId As Core.Properties.cmisProperty = FindProperty(Constants.CmisPredefinedPropertyNames.ObjectTypeId)
            If objectTypeId IsNot Nothing Then
               Dim declaringType As String = TryCast(objectTypeId.Value, String)
               For Each [property] As Core.Properties.cmisProperty In _properties
                  If [property] IsNot Nothing Then
                     Dim extendedProperties As Dictionary(Of String, Object) = [property].ExtendedProperties
                     If Not extendedProperties.ContainsKey(Constants.ExtendedProperties.DeclaringType) Then
                        extendedProperties.Add(Constants.ExtendedProperties.DeclaringType, declaringType)
                     End If
                  End If
               Next
            End If
         End If
      End Sub
#End Region

      ''' <summary>
      ''' Appends a new cmisProperty-instance to the Properties-array
      ''' </summary>
      ''' <param name="cmisProperty"></param>
      ''' <remarks></remarks>
      Public Sub Append(cmisProperty As Core.Properties.cmisProperty)
         If cmisProperty IsNot Nothing Then
            If _properties Is Nothing Then
               Me.Properties = New Core.Properties.cmisProperty() {cmisProperty}
            Else
               Dim length As Integer = _properties.Length
               Dim properties As Core.Properties.cmisProperty() = CType(Array.CreateInstance(GetType(Core.Properties.cmisProperty), length + 1), Core.Properties.cmisProperty())

               Array.Copy(_properties, properties, length)
               properties(length) = cmisProperty
               Me.Properties = properties
            End If
         End If
      End Sub

      ''' <summary>
      ''' Updates the entries of result: if a key exists in the properties-collection the corresponding property
      ''' is copied to the value of the dictionary-entry
      ''' </summary>
      ''' <param name="result"></param>
      ''' <param name="ignoreCase"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CopyTo(result As Dictionary(Of String, Core.Properties.cmisProperty), ignoreCase As Boolean) As Dictionary(Of String, Core.Properties.cmisProperty)
         Dim map As New Dictionary(Of String, String) 'map for lowerCase to requested propertyDefinitionId

         If result Is Nothing Then result = New Dictionary(Of String, Core.Properties.cmisProperty)
         'mapping
         For Each propertyDefinitionId As String In result.Keys
            Dim key As String = If(ignoreCase, propertyDefinitionId.ToLowerInvariant(), propertyDefinitionId)

            If Not map.ContainsKey(key) Then
               map.Add(key, propertyDefinitionId)
            End If
         Next
         'search for property-instances
         If _properties IsNot Nothing Then
            For Each [property] As Core.Properties.cmisProperty In _properties
               If [property] IsNot Nothing Then
                  Dim propertyDefinitionId As String = [property].PropertyDefinitionId

                  If Not String.IsNullOrEmpty(propertyDefinitionId) Then
                     Dim key As String = If(ignoreCase, propertyDefinitionId.ToLowerInvariant(), propertyDefinitionId)

                     If map.ContainsKey(key) Then result(map(key)) = [property]
                  End If
               End If
            Next
         End If

         Return result
      End Function

      ''' <summary>
      ''' Returns _properties.Length or 0, if _properties is null
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Count As Integer
         Get
            Return If(_properties Is Nothing, 0, _properties.Length)
         End Get
      End Property

      ''' <summary>
      ''' Search for a specified extension type
      ''' </summary>
      Public Function FindExtension(Of TExtension As Extensions.Extension)() As TExtension
         If _extensions IsNot Nothing Then
            For Each extension As Extensions.Extension In _extensions
               If TypeOf extension Is TExtension Then Return CType(extension, TExtension)
            Next
         End If

         'nothing found
         Return Nothing
      End Function

      ''' <summary>
      ''' Returns a dictionary that contains an entry for all propertyDefinitionIds that are not null or empty
      ''' </summary>
      ''' <param name="ignoreCase"></param>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function FindProperties(ignoreCase As Boolean, ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Core.Properties.cmisProperty)
         Dim retVal As New Dictionary(Of String, Core.Properties.cmisProperty)
         Dim map As New Dictionary(Of String, String) 'map for lowerCase to requested propertyDefinitionId

         'the returned dictionary MUST support all requested valid propertyDefinitionIds (not null or empty)
         If propertyDefinitionIds IsNot Nothing Then
            For Each propertyDefinitionId As String In propertyDefinitionIds
               If Not String.IsNullOrEmpty(propertyDefinitionId) Then
                  Dim key As String = If(ignoreCase, propertyDefinitionId.ToLowerInvariant(), propertyDefinitionId)

                  If Not map.ContainsKey(key) Then
                     map.Add(key, propertyDefinitionId)
                     retVal.Add(propertyDefinitionId, Nothing)
                  End If
               End If
            Next
         End If
         'search for property-instances
         If _properties IsNot Nothing Then
            For Each [property] As Core.Properties.cmisProperty In _properties
               If [property] IsNot Nothing Then
                  Dim propertyDefinitionId As String = [property].PropertyDefinitionId

                  If Not String.IsNullOrEmpty(propertyDefinitionId) Then
                     Dim key As String = If(ignoreCase, propertyDefinitionId.ToLowerInvariant(), propertyDefinitionId)

                     If map.ContainsKey(key) Then retVal(map(key)) = [property]
                  End If
               End If
            Next
         End If

         Return retVal
      End Function

      Public Function FindProperty(propertyDefinitionId As String, Optional ignoreCase As Boolean = True) As Core.Properties.cmisProperty
         If Not String.IsNullOrEmpty(propertyDefinitionId) AndAlso _properties IsNot Nothing Then
            For Each retVal As Core.Properties.cmisProperty In _properties
               If retVal IsNot Nothing AndAlso String.Compare(retVal.PropertyDefinitionId, propertyDefinitionId, ignoreCase) = 0 Then Return retVal
            Next
         End If

         'unable to find property
         Return Nothing
      End Function

      Public Function FindProperty(Of TResult As Core.Properties.cmisProperty)(propertyDefinitionId As String, Optional ignoreCase As Boolean = True) As TResult
         Return TryCast(FindProperty(propertyDefinitionId, ignoreCase), TResult)
      End Function

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns></returns>
      ''' <remarks>The given propertyDefinitionIds handled casesensitive, if there is
      ''' none at all, all properties of this instance will be returned</remarks>
      Public Function GetProperties(ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisProperty)
         Return GetProperties(enumKeySyntax.original, propertyDefinitionIds)
      End Function

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="ignoreCase">If True each propertyDefinitionId is compared case insensitive</param>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns>Dictionary of all existing properties specified by propertyDefinitionsIds.
      ''' Notice: if ignoreCase is set to True, then the keys of the returned dictionary are lowercase</returns>
      ''' <remarks>If there are no propertyDefinitionIds defined, all properties of this instance will be returned</remarks>
      Public Function GetProperties(ignoreCase As Boolean, ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisProperty)
         Return GetProperties(If(ignoreCase, enumKeySyntax.lowerCase, enumKeySyntax.original), propertyDefinitionIds)
      End Function

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="keySyntax">If lowerCase-bit is set each propertyDefinitionId is compared case insensitive</param>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns>Dictionary of all existing properties specified by propertyDefinitionsIds.
      ''' Notice: if keySyntax is set to lowerCase, then the keys of the returned dictionary are lowercase</returns>
      ''' <remarks>If there are no propertyDefinitionIds defined, all properties of this instance will be returned</remarks>
      Public Function GetProperties(keySyntax As enumKeySyntax, ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisProperty)
         Dim retVal As New Dictionary(Of String, Properties.cmisProperty)
         Dim ignoreCase As Boolean = ((keySyntax And enumKeySyntax.searchIgnoreCase) = enumKeySyntax.searchIgnoreCase)
         Dim lowerCase As Boolean = ((keySyntax And enumKeySyntax.lowerCase) = enumKeySyntax.lowerCase)
         Dim originalCase As Boolean = ((keySyntax And enumKeySyntax.original) = enumKeySyntax.original)

         'collect requested properties
         If _properties IsNot Nothing Then
            'collect requested propertyDefinitionIds
            Dim verifyIds As New HashSet(Of String)

            If propertyDefinitionIds Is Nothing OrElse propertyDefinitionIds.Length = 0 Then
               verifyIds.Add("*")
            Else
               For Each propertyDefinitionId As String In propertyDefinitionIds
                  If propertyDefinitionId Is Nothing Then propertyDefinitionId = ""
                  If ignoreCase Then propertyDefinitionId = propertyDefinitionId.ToLowerInvariant()
                  verifyIds.Add(propertyDefinitionId)
               Next
            End If

            'collect requested properties
            For Each [property] As Properties.cmisProperty In _properties
               Dim originalName As String = [property].PropertyDefinitionId
               Dim name As String

               If originalName Is Nothing Then
                  originalName = ""
                  name = ""
               ElseIf ignoreCase Then
                  name = originalName.ToLowerInvariant()
               Else
                  name = originalName
               End If
               If (verifyIds.Contains(name) OrElse verifyIds.Contains("*")) Then
                  If lowerCase AndAlso Not retVal.ContainsKey(name) Then retVal.Add(name, [property])
                  If originalCase AndAlso Not retVal.ContainsKey(originalName) Then retVal.Add(originalName, [property])
               End If
            Next
         End If

         Return retVal
      End Function

      Private _propertiesAsReadOnly As New ccg.ArrayMapper(Of cmisPropertiesType, Core.Properties.cmisProperty)(Me,
                                                                                                                "Properties", Function() _properties,
                                                                                                                "PropertyDefinitionId", Function([property]) [property].PropertyDefinitionId)
      ''' <summary>
      ''' Access to properties via index or PropertyDefinitionId
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property PropertiesAsReadOnly As ccg.ArrayMapper(Of cmisPropertiesType, Core.Properties.cmisProperty)
         Get
            Return _propertiesAsReadOnly
         End Get
      End Property

      ''' <summary>
      ''' Remove the specified property
      ''' </summary>
      ''' <param name="propertyDefinitionId"></param>
      ''' <param name="ignoreCase"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function RemoveProperty(propertyDefinitionId As String, Optional ignoreCase As Boolean = True) As Boolean
         Dim retVal As Boolean = False

         If _properties IsNot Nothing Then
            Dim properties As New List(Of Core.Properties.cmisProperty)(_properties)

            For index As Integer = properties.Count - 1 To 0 Step -1
               If properties(index) Is Nothing OrElse
                  String.Compare(properties(index).PropertyDefinitionId, propertyDefinitionId, ignoreCase) = 0 Then
                  properties.RemoveAt(index)
                  retVal = True
               End If
            Next

            If retVal Then Me.Properties = If(properties.Count > 0, properties.ToArray, Nothing)
         End If

         Return retVal
      End Function

   End Class
End Namespace