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
   ''' Converter for SetAspects-instances
   ''' </summary>
   ''' <remarks></remarks>
   Public Class SetAspectsConverter
      Inherits Serialization.Generic.JavaScriptConverter(Of CmisObjectModel.Extensions.Alfresco.SetAspects)

#Region "Constructors"
      Public Sub New()
         MyBase.New(New JSON.Serialization.Generic.DefaultObjectResolver(Of CmisObjectModel.Extensions.Alfresco.SetAspects))
      End Sub
      Public Sub New(objectResolver As Serialization.Generic.ObjectResolver(Of CmisObjectModel.Extensions.Alfresco.SetAspects))
         MyBase.New(objectResolver)
      End Sub
#End Region

      Protected Overloads Overrides Sub Deserialize(context As SerializationContext)
         With context
            Dim setAspects As IEnumerable =
               If(.Dictionary.ContainsKey("aspects"), TryCast(.Dictionary("aspects"), IEnumerable), Nothing)

            If setAspects IsNot Nothing Then
               Dim propertiesType As Type = GetType(CmisObjectModel.Core.Collections.cmisPropertiesType)
               Dim propertiesConverter As JSON.Serialization.JavaScriptConverter =
                  context.Serializer.GetJavaScriptConverter(propertiesType)
               Dim aspects As New List(Of CmisObjectModel.Extensions.Alfresco.SetAspects.Aspect)

               For Each rawAspect As Object In setAspects
                  Dim aspect As IDictionary(Of String, Object) = TryCast(rawAspect, IDictionary(Of String, Object))

                  If aspect IsNot Nothing Then
                     Dim action As CmisObjectModel.Extensions.Alfresco.SetAspects.enumSetAspectsAction =
                        ReadEnum(aspect, "action", CmisObjectModel.Extensions.Alfresco.SetAspects.enumSetAspectsAction.aspectsToAdd)
                     Dim aspectName As String = If(aspect.ContainsKey("aspectName"), CStr(aspect.ContainsKey("aspectName")), Nothing)
                     Dim propertyCollection As IDictionary(Of String, Object) =
                        If(aspect.ContainsKey("properties"), TryCast(aspect("properties"), IDictionary(Of String, Object)), Nothing)
                     Dim properties As CmisObjectModel.Core.Collections.cmisPropertiesType =
                        If(propertyCollection Is Nothing, Nothing,
                           TryCast(propertiesConverter.Deserialize(propertyCollection, propertiesType, .Serializer), 
                                   CmisObjectModel.Core.Collections.cmisPropertiesType))
                     aspects.Add(New CmisObjectModel.Extensions.Alfresco.SetAspects.Aspect(action, aspectName, properties))
                  End If
               Next

               .Object.Aspects = aspects.ToArray()
            End If
         End With
      End Sub

      Protected Overloads Overrides Sub Serialize(context As SerializationContext)
         With context
            If .Object.Aspects IsNot Nothing Then
               Dim propertiesType As Type = GetType(CmisObjectModel.Core.Collections.cmisPropertiesType)
               Dim propertiesConverter As JSON.Serialization.JavaScriptConverter =
                  context.Serializer.GetJavaScriptConverter(propertiesType)
               Dim aspects As New List(Of IDictionary(Of String, Object))

               For Each setAspect As CmisObjectModel.Extensions.Alfresco.SetAspects.Aspect In .Object.Aspects
                  If setAspect IsNot Nothing Then
                     Dim aspect As New Dictionary(Of String, Object)

                     aspect.Add("action", setAspect.Action.GetName())
                     aspect.Add("aspectName", setAspect.AspectName)
                     If setAspect.Properties IsNot Nothing Then
                        aspect.Add("properties", propertiesConverter.Serialize(setAspect.Properties, .Serializer))
                     End If
                     aspects.Add(aspect)
                  End If
               Next
               .Add("aspects", aspects.ToArray())
            End If
            .Add("extensionTypeName", .Object.ExtensionTypeName)
         End With
      End Sub

   End Class
End Namespace