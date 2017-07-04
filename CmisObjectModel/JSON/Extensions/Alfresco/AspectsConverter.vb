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
Imports swss = System.Web.Script.Serialization

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON.Extensions.Alfresco
   ''' <summary>
   ''' Converter for Aspects-instances
   ''' </summary>
   ''' <remarks></remarks>
   Public Class AspectsConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Extensions.Alfresco.Aspects)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Extensions.Alfresco.Aspects))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Extensions.Alfresco.Aspects))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            Dim appliedAspects As IEnumerable =
               If(.Dictionary.ContainsKey("appliedAspects"), TryCast(.Dictionary("appliedAspects"), IEnumerable), Nothing)

            If appliedAspects IsNot Nothing Then
               Dim propertiesType As Type = GetType(CmisObjectModel.Core.Collections.cmisPropertiesType)
               Dim propertiesConverter As JSON.Serialization.JavaScriptConverter =
                  context.Serializer.GetJavaScriptConverter(propertiesType)
               Dim aspects As New List(Of CmisObjectModel.Extensions.Alfresco.Aspects.Aspect)

               For Each rawAspect As Object In appliedAspects
                  Dim aspect As IDictionary(Of String, Object) = TryCast(rawAspect, IDictionary(Of String, Object))

                  If aspect IsNot Nothing Then
                     Dim aspectName As String = If(aspect.ContainsKey("aspectName"), CStr(aspect.ContainsKey("aspectName")), Nothing)
                     Dim propertyCollection As IDictionary(Of String, Object) =
                        If(aspect.ContainsKey("properties"), TryCast(aspect("properties"), IDictionary(Of String, Object)), Nothing)
                     Dim properties As CmisObjectModel.Core.Collections.cmisPropertiesType =
                        If(propertyCollection Is Nothing, Nothing,
                           TryCast(propertiesConverter.Deserialize(propertyCollection, propertiesType, .Serializer), 
                                   CmisObjectModel.Core.Collections.cmisPropertiesType))
                     aspects.Add(New CmisObjectModel.Extensions.Alfresco.Aspects.Aspect(aspectName, properties))
                  End If
               Next

               .Object.AppliedAspects = aspects.ToArray()
            End If
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            If .Object.AppliedAspects IsNot Nothing Then
               Dim propertiesType As Type = GetType(CmisObjectModel.Core.Collections.cmisPropertiesType)
               Dim propertiesConverter As JSON.Serialization.JavaScriptConverter =
                  context.Serializer.GetJavaScriptConverter(propertiesType)
               Dim aspects As New List(Of IDictionary(Of String, Object))

               For Each appliedAspect As CmisObjectModel.Extensions.Alfresco.Aspects.Aspect In .Object.AppliedAspects
                  If appliedAspect IsNot Nothing Then
                     Dim aspect As New Dictionary(Of String, Object)

                     aspect.Add("aspectName", appliedAspect.AspectName)
                     If appliedAspect.Properties IsNot Nothing Then
                        aspect.Add("properties", propertiesConverter.Serialize(appliedAspect.Properties, .Serializer))
                     End If
                     aspects.Add(aspect)
                  End If
               Next
               .Add("appliedAspects", aspects.ToArray())
            End If
            .Add("extensionTypeName", .Object.ExtensionTypeName)
         End With
      End Sub

   End Class
End Namespace