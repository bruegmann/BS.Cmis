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
Imports CmisObjectModel.Constants
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Extensions.Alfresco
   ''' <summary>
   ''' Support for Alfresco Aspect extensions (in properties-collection)
   ''' </summary>
   ''' <remarks></remarks>
   <sxs.XmlRoot("aspects", Namespace:=Constants.Namespaces.alf),
    Attributes.CmisTypeInfo("alf:aspects", Nothing, "aspects"),
    Attributes.JavaScriptConverter(GetType(JSON.Extensions.Alfresco.AspectsConverter))>
   Public Class Aspects
      Inherits Extension

#Region "Constructors"
      Public Sub New()
      End Sub

      Public Sub New(ParamArray appliedAspects As Aspect())
         _appliedAspects = appliedAspects
      End Sub
#End Region

#Region "Helper classes"
      Public Class Aspect

         Public Sub New(aspectName As String)
            Me.AspectName = aspectName
         End Sub
         Public Sub New(aspectName As String, properties As Core.Collections.cmisPropertiesType)
            Me.AspectName = aspectName
            Me.Properties = properties
            Seal()
         End Sub

         Public ReadOnly AspectName As String
         Public Properties As Core.Collections.cmisPropertiesType

         ''' <summary>
         ''' Sets the declaringType-ExtendedProperty of all properties within this instance
         ''' </summary>
         ''' <remarks></remarks>
         Public Sub Seal()
            If Not String.IsNullOrEmpty(AspectName) Then
               Dim properties As Core.Properties.cmisProperty() = If(Me.Properties Is Nothing, Nothing, Me.Properties.Properties)

               If properties IsNot Nothing Then
                  For Each [property] As Core.Properties.cmisProperty In properties
                     If [property] IsNot Nothing Then
                        With [property].ExtendedProperties
                           If .ContainsKey(Constants.ExtendedProperties.DeclaringType) Then
                              .Item(Constants.ExtendedProperties.DeclaringType) = AspectName
                           Else
                              .Add(Constants.ExtendedProperties.DeclaringType, AspectName)
                           End If
                        End With
                     End If
                  Next
               End If
            End If
         End Sub

      End Class
#End Region

#Region "IXmlSerializable"
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         Dim currentAspect As Aspect = Nothing
         Dim appliedAspects As New List(Of Aspect)

         reader.MoveToContent()
         While reader.IsStartElement
            If String.Compare(reader.NamespaceURI, Namespaces.alf, True) = 0 Then
               If String.Compare(reader.LocalName, "appliedAspects", True) = 0 Then
                  'seal the current aspect
                  If currentAspect IsNot Nothing Then appliedAspects.Add(currentAspect)
                  'start the next aspect
                  currentAspect = New Aspect(reader.ReadElementString())
               ElseIf String.Compare(reader.LocalName, "properties", True) = 0 Then
                  'ensure aspect
                  If currentAspect Is Nothing Then currentAspect = New Aspect(Nothing)
                  'append aspects - properties
                  currentAspect.Properties = New Core.Collections.cmisPropertiesType
                  currentAspect.Properties.ReadXml(reader)
                  appliedAspects.Add(currentAspect)
                  currentAspect.Seal()
                  currentAspect = Nothing
               End If
            End If
         End While
         'maybe there is an aspect-Item left
         If currentAspect IsNot Nothing Then appliedAspects.Add(currentAspect)
         _appliedAspects = If(appliedAspects.Count = 0, Nothing, appliedAspects.ToArray())
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If _appliedAspects IsNot Nothing Then
            For Each item As Aspect In _appliedAspects
               If item IsNot Nothing Then
                  If Not String.IsNullOrEmpty(item.AspectName) Then
                     WriteElement(writer, attributeOverrides, "appliedAspects", Namespaces.alf, item.AspectName)
                  End If
                  If item.Properties IsNot Nothing Then
                     WriteElement(writer, attributeOverrides, "properties", Namespaces.alf, item.Properties)
                  End If
               End If
            Next
         End If
      End Sub
#End Region

      Private _appliedAspects As Aspect()
      Public Overridable Property AppliedAspects As Aspect()
         Get
            Return _appliedAspects
         End Get
         Set(value As Aspect())
            If _appliedAspects IsNot value Then
               Dim oldValue As Aspect() = _appliedAspects
               _appliedAspects = value
               OnPropertyChanged("AppliedAspects", value, oldValue)
            End If
         End Set
      End Property 'AppliedAspects
   End Class
End Namespace