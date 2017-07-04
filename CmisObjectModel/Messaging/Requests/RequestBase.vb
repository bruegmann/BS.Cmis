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
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Messaging.Requests
   Public MustInherit Class RequestBase
      Inherits Serialization.XmlSerializable

#Region "Constructors"
      Shared Sub New()
         ExploreAssembly(GetType(RequestBase).Assembly)
      End Sub
      Protected Sub New()
      End Sub
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub

      ''' <summary>
      ''' Creates a RequestBase-instance from the current node of the reader-object using the
      ''' name of the current node to determine the suitable type
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function CreateInstance(reader As System.Xml.XmlReader) As RequestBase
         reader.MoveToContent()

         Dim nodeName As String = reader.LocalName
         If nodeName <> "" Then
            nodeName = nodeName.ToLowerInvariant()
            If _factories.ContainsKey(nodeName) Then
               Dim retVal As RequestBase = _factories(nodeName).CreateInstance()

               If retVal IsNot Nothing Then
                  Dim attributeOverrides As Serialization.XmlAttributeOverrides = Serialization.XmlAttributeOverrides.GetInstance(reader)

                  If attributeOverrides IsNot Nothing Then
                     'duplicate xmlRoot for retVal-type, if available
                     Dim xmlRoot As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(GetType(RequestBase))

                     If xmlRoot IsNot Nothing Then attributeOverrides.XmlRoot(retVal.GetType()) = xmlRoot
                  End If
                  retVal.ReadXml(reader)
                  Return retVal
               End If
            End If
         End If

         'current node doesn't describe a RequestBase-instance
         Return Nothing
      End Function

      ''' <summary>
      ''' Searches in assembly for types supporting requests
      ''' </summary>
      ''' <param name="assembly"></param>
      ''' <remarks></remarks>
      Public Shared Function ExploreAssembly(assembly As System.Reflection.Assembly) As Boolean
         Try
            Dim baseType As Type = GetType(RequestBase)

            'explore the complete assembly if possible
            If assembly IsNot Nothing Then
               For Each type As Type In assembly.GetTypes()
                  Try
                     Dim attrs As Object() = type.GetCustomAttributes(GetType(sxs.XmlRootAttribute), False)

                     If baseType.IsAssignableFrom(type) AndAlso
                        Not type.IsAbstract AndAlso type.GetConstructor(New Type() {}) IsNot Nothing Then
                        Dim elementName As String = If(attrs IsNot Nothing AndAlso attrs.Length > 0, CType(attrs(0), sxs.XmlRootAttribute).ElementName, Nothing)

                        If Not String.IsNullOrEmpty(elementName) AndAlso Not _factories.ContainsKey(elementName) Then
                           'create factory for this valid type
                           _factories.Add(elementName.ToLowerInvariant(),
                                          CType(System.Activator.CreateInstance(_genericTypeDefinition.MakeGenericType(baseType, type)), Generic.Factory(Of RequestBase)))

                        End If
                     End If
                  Catch
                  End Try
               Next
            End If
            Return True
         Catch
            Return False
         End Try
      End Function

      Protected Shared _factories As New Dictionary(Of String, CmisObjectModel.Common.Generic.Factory(Of RequestBase))
      ''' <summary>
      ''' GetType(Generic.Factory(Of RequestBase, TDerivedFromRequestBase))
      ''' </summary>
      ''' <remarks></remarks>
      Private Shared _genericTypeDefinition As Type = GetType(CmisObjectModel.Common.Generic.Factory(Of RequestBase, addObjectToFolder)).GetGenericTypeDefinition
#End Region

      ''' <summary>
      ''' Returns value if not null or empty, otherwise defaultValue
      ''' </summary>
      ''' <param name="value"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overloads Function Read(value As String, defaultValue As String) As String
         Return If(String.IsNullOrEmpty(value), defaultValue, value)
      End Function

      ''' <summary>
      ''' Converts value if not null or empty, otherwise defaultValue
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="value"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overloads Function Read(Of T)(value As String, defaultValue As T) As T
         Return If(String.IsNullOrEmpty(value), defaultValue, ConvertBack(value, defaultValue))
      End Function

      ''' <summary>
      ''' Converts value to TEnum if not null or empty, otherwise defaultValue
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="value"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overloads Function ReadEnum(Of TEnum As Structure)(value As String, defaultValue As TEnum) As TEnum
         Dim enumValue As TEnum

         Return If(String.IsNullOrEmpty(value) OrElse Not TryParse(value, enumValue, True), defaultValue, enumValue)
      End Function

      Protected Overloads Function ReadOptionalEnum(Of TEnum As Structure)(value As String, defaultValue As TEnum?) As TEnum?
         Dim enumValue As TEnum

         Return If(String.IsNullOrEmpty(value) OrElse Not TryParse(value, enumValue, True), defaultValue, enumValue)
      End Function

      ''' <summary>
      ''' Reads the queryStringParameters of the current request and copies the values to
      ''' corresponding properties
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Public Overridable Sub ReadQueryString(repositoryId As String)
      End Sub

   End Class
End Namespace