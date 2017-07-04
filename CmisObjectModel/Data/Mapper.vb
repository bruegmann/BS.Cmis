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
Imports ca = CmisObjectModel.AtomPub
Imports ss = System.ServiceModel
Imports sss = System.ServiceModel.Syndication
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml

Namespace CmisObjectModel.Data
   ''' <summary>
   ''' Simple mapper 
   ''' </summary>
   ''' <remarks></remarks>
   Public Class Mapper

      Public Sub New(Optional allowedDirections As enumMapDirection = enumMapDirection.none)
         _allowedDirections = allowedDirections
      End Sub

      Private _allowedDirections As enumMapDirection
      Public Property AllowedDirections As enumMapDirection
         Get
            Return _allowedDirections
         End Get
         Set(value As enumMapDirection)
            _allowedDirections = (value And enumMapDirection.bidirectional)
         End Set
      End Property

      ''' <summary>
      ''' Remove all PropertyValueConverters within this mapper-instance
      ''' </summary>
      Public Sub ClearPropertyValueConverters()
         _propertyValueConverters.Clear()
      End Sub

      ''' <summary>
      ''' Maps the values of properties for the remote system (direction = outgoing) or current system (direction = incoming) and
      ''' returns a delegate to rollback the mapping
      ''' </summary>
      ''' <param name="properties"></param>
      ''' <param name="direction"></param>
      ''' <returns>Delegate to rollback the mapping</returns>
      ''' <remarks></remarks>
      Public Function MapProperties(properties As Core.Collections.cmisPropertiesType, direction As enumMapDirection) As Action
         Dim rollbackSettings As New Dictionary(Of Core.Properties.cmisProperty, Object())

         direction = (direction And _allowedDirections)
         If properties IsNot Nothing AndAlso direction <> enumMapDirection.none Then
            MapProperties(properties, direction, rollbackSettings)
         End If
         If rollbackSettings.Count = 0 Then
            Return Nothing
         Else
            Return Sub()
                      For Each de As KeyValuePair(Of Core.Properties.cmisProperty, Object()) In rollbackSettings
                         de.Key.SetValuesSilent(de.Value)
                      Next
                   End Sub
         End If
      End Function
      Friend Sub MapProperties(properties As Core.Collections.cmisPropertiesType, direction As enumMapDirection,
                               rollbackSettings As Dictionary(Of Core.Properties.cmisProperty, Object()))
         'property mapping for current properties-instance
         If properties.Properties IsNot Nothing Then
            For Each [property] As Core.Properties.cmisProperty In properties
               Dim converter As PropertyValueConverter = If([property] Is Nothing OrElse rollbackSettings.ContainsKey([property]),
                                                            Nothing, Me.PropertyValueConverter([property].PropertyDefinitionId))
               If converter IsNot Nothing Then
                  rollbackSettings.Add([property],
                                       [property].SetValuesSilent(If(direction = enumMapDirection.incoming,
                                                                     converter.Convert([property].Values), converter.ConvertBack([property].Values))))
               End If
            Next
         End If
         'property mapping for rowCollections
         If properties.Extensions IsNot Nothing Then
            For Each extension As Extensions.Extension In properties.Extensions
               If TypeOf extension Is Extensions.Data.RowCollection Then
                  Dim rows As Extensions.Data.RowCollection = DirectCast(extension, Extensions.Data.RowCollection)

                  If rows.Rows IsNot Nothing Then
                     For Each row As Extensions.Data.Row In rows.Rows
                        If row IsNot Nothing Then row.MapProperties(Me, direction, rollbackSettings)
                     Next
                  End If
               End If
            Next
         End If
      End Sub

      Private _propertyValueConverters As New Dictionary(Of String, PropertyValueConverter)
      ''' <summary>
      ''' Gets or sets the propertyValueConverter for a specified propertyDefinitionId
      ''' </summary>
      ''' <param name="propertyDefinitionId"></param>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Property PropertyValueConverter(propertyDefinitionId As String) As PropertyValueConverter
         Get
            Return If(String.IsNullOrEmpty(propertyDefinitionId) OrElse Not _propertyValueConverters.ContainsKey(propertyDefinitionId),
                      Nothing, _propertyValueConverters(propertyDefinitionId))
         End Get
         Set(value As PropertyValueConverter)
            If Not String.IsNullOrEmpty(propertyDefinitionId) Then
               If _propertyValueConverters.ContainsKey(propertyDefinitionId) Then
                  If value Is Nothing Then
                     _propertyValueConverters.Remove(propertyDefinitionId)
                  ElseIf value.LocalType Is value.RemoteType Then
                     'for mapper instances there MUST NOT be a type-conversion defined
                     _propertyValueConverters(propertyDefinitionId) = value
                  End If
               ElseIf value IsNot Nothing AndAlso value.LocalType Is value.RemoteType Then
                  'for mapper instances there MUST NOT be a type-conversion defined
                  _propertyValueConverters.Add(propertyDefinitionId, value)
               End If
            End If
         End Set
      End Property

   End Class
End Namespace