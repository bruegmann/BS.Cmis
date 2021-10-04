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
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Common
   ''' <summary>
   ''' Factory of cmis predefined PropertyDefinition-instances
   ''' </summary>
   ''' <remarks></remarks>
   Public Class PredefinedPropertyDefinitionFactory

      Protected Shared _predefinedRequired As New Dictionary(Of String, Boolean) From {
         {Constants.CmisPredefinedPropertyNames.AllowedChildObjectTypeIds, False},
         {Constants.CmisPredefinedPropertyNames.ChangeToken, False},
         {Constants.CmisPredefinedPropertyNames.CreatedBy, True},
         {Constants.CmisPredefinedPropertyNames.CreationDate, True},
         {Constants.CmisPredefinedPropertyNames.Extensions.ForeignChangeToken, False},
         {Constants.CmisPredefinedPropertyNames.Extensions.ForeignObjectId, False},
         {Constants.CmisPredefinedPropertyNames.Extensions.SyncRequired, False},
         {Constants.CmisPredefinedPropertyNames.LastModificationDate, True},
         {Constants.CmisPredefinedPropertyNames.LastModifiedBy, True},
         {Constants.CmisPredefinedPropertyNames.ObjectId, True}}
      Protected _localNamespace As String
      Protected _inherited As Boolean
      Protected _isBaseType As Boolean
      Public Sub New(localNamespace As String, Optional isBaseType As Boolean = True)
         _localNamespace = localNamespace
         _inherited = Not isBaseType
         _isBaseType = isBaseType
      End Sub

#Region "PropertyDefinitionFactories"
      ''' <summary>
      ''' Returns a PropertyBooleanDefinition-object
      ''' </summary>
      ''' <param name="id"></param>
      ''' <param name="localName"></param>
      ''' <param name="displayName"></param>
      ''' <param name="queryName">ignored for base types</param>
      ''' <param name="required"></param>
      ''' <param name="orderable"></param>
      ''' <param name="cardinality"></param>
      ''' <param name="updatability"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyBooleanDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                            required As Boolean, orderable As Boolean,
                                                            cardinality As Core.enumCardinality, updatability As Core.enumUpdatability) As ccdp.cmisPropertyBooleanDefinitionType
         Return New ccdp.cmisPropertyBooleanDefinitionType(id, localName, _localNamespace, displayName,
                                                           If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                           required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                           cardinality, updatability) With {.Description = displayName}
      End Function
      ''' <summary>
      ''' Returns a PropertyDateTimeDefinition-object
      ''' </summary>
      ''' <param name="id"></param>
      ''' <param name="localName"></param>
      ''' <param name="displayName"></param>
      ''' <param name="queryName">ignored for base types</param>
      ''' <param name="required"></param>
      ''' <param name="orderable"></param>
      ''' <param name="cardinality"></param>
      ''' <param name="updatability"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyDateTimeDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                             required As Boolean, orderable As Boolean,
                                                             cardinality As Core.enumCardinality, updatability As Core.enumUpdatability) As ccdp.cmisPropertyDateTimeDefinitionType
         Return New ccdp.cmisPropertyDateTimeDefinitionType(id, localName, _localNamespace, displayName,
                                                            If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                            required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                            cardinality, updatability) With {.Description = displayName}
      End Function
      ''' <summary>
      ''' Returns a PropertyDecimalDefinition-object
      ''' </summary>
      ''' <param name="queryName">ignored for base types</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyDecimalDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                            required As Boolean, orderable As Boolean,
                                                            cardinality As Core.enumCardinality, updatability As Core.enumUpdatability,
                                                            maxValue As Decimal, minValue As Decimal) As ccdp.cmisPropertyDecimalDefinitionType
         Return New ccdp.cmisPropertyDecimalDefinitionType(id, localName, _localNamespace, displayName,
                                                           If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                           required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                           cardinality, updatability) With {.Description = displayName}
      End Function
      ''' <summary>
      ''' Returns a PropertyDoubleDefinition-object
      ''' </summary>
      ''' <param name="queryName">ignored for base types</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyDoubleDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                           required As Boolean, orderable As Boolean,
                                                           cardinality As Core.enumCardinality, updatability As Core.enumUpdatability,
                                                           maxValue As Double, minValue As Double) As ccdp.cmisPropertyDoubleDefinitionType
         Return New ccdp.cmisPropertyDoubleDefinitionType(id, localName, _localNamespace, displayName,
                                                          If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                          required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                          cardinality, updatability) With {.Description = displayName}
      End Function
      ''' <summary>
      ''' Returns a PropertyHtmlDefinition-object
      ''' </summary>
      ''' <param name="queryName">ignored for base types</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyHtmlDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                         required As Boolean, orderable As Boolean,
                                                         cardinality As Core.enumCardinality, updatability As Core.enumUpdatability) As ccdp.cmisPropertyHtmlDefinitionType
         Return New ccdp.cmisPropertyHtmlDefinitionType(id, localName, _localNamespace, displayName,
                                                        If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                        required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                        cardinality, updatability) With {.Description = displayName}
      End Function
      ''' <summary>
      ''' Returns a PropertyIdDefinition-object
      ''' </summary>
      ''' <param name="id"></param>
      ''' <param name="localName"></param>
      ''' <param name="displayName"></param>
      ''' <param name="queryName">ignored for base types</param>
      ''' <param name="required"></param>
      ''' <param name="orderable"></param>
      ''' <param name="cardinality"></param>
      ''' <param name="updatability"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyIdDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                       required As Boolean, orderable As Boolean,
                                                       cardinality As Core.enumCardinality, updatability As Core.enumUpdatability) As ccdp.cmisPropertyIdDefinitionType
         Return New ccdp.cmisPropertyIdDefinitionType(id, localName, _localNamespace, displayName,
                                                      If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                      required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                      cardinality, updatability) With {.Description = displayName}
      End Function
      ''' <summary>
      ''' Returns a PropertyIntegerDefinition-object
      ''' </summary>
      ''' <param name="id"></param>
      ''' <param name="localName"></param>
      ''' <param name="displayName"></param>
      ''' <param name="queryName">ignored for base types</param>
      ''' <param name="required"></param>
      ''' <param name="orderable"></param>
      ''' <param name="cardinality"></param>
      ''' <param name="updatability"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyIntegerDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                            required As Boolean, orderable As Boolean,
                                                            cardinality As Core.enumCardinality, updatability As Core.enumUpdatability,
                                                            maxValue As xs_Integer, minValue As xs_Integer) As ccdp.cmisPropertyIntegerDefinitionType
         Return New ccdp.cmisPropertyIntegerDefinitionType(id, localName, _localNamespace, displayName,
                                                           If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                           required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                           cardinality, updatability) With {.Description = displayName, .MaxValue = maxValue, .MinValue = minValue}
      End Function
      ''' <summary>
      ''' Returns a PropertyStringDefinition-object
      ''' </summary>
      ''' <param name="id"></param>
      ''' <param name="localName"></param>
      ''' <param name="displayName"></param>
      ''' <param name="queryName">ignored for base types</param>
      ''' <param name="required"></param>
      ''' <param name="orderable"></param>
      ''' <param name="cardinality"></param>
      ''' <param name="updatability"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyStringDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                           required As Boolean, orderable As Boolean,
                                                           cardinality As Core.enumCardinality, updatability As Core.enumUpdatability) As ccdp.cmisPropertyStringDefinitionType
         Return New ccdp.cmisPropertyStringDefinitionType(id, localName, _localNamespace, displayName,
                                                          If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                          required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                          cardinality, updatability) With {.Description = displayName}
      End Function
      ''' <summary>
      ''' Returns a PropertyUriDefinition-object
      ''' </summary>
      ''' <param name="queryName">ignored for base types</param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function PropertyUriDefinition(id As String, localName As String, displayName As String, queryName As String,
                                                        required As Boolean, orderable As Boolean,
                                                        cardinality As Core.enumCardinality, updatability As Core.enumUpdatability) As ccdp.cmisPropertyUriDefinitionType
         Return New ccdp.cmisPropertyUriDefinitionType(id, localName, _localNamespace, displayName,
                                                       If(_isBaseType OrElse String.IsNullOrEmpty(queryName), id, queryName),
                                                       required, _inherited, If(_predefinedRequired.ContainsKey(id), _predefinedRequired(id), queryName <> ""), orderable,
                                                       cardinality, updatability) With {.Description = displayName}
      End Function
#End Region

      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the allowedChildObjectTypeIds-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function AllowedChildObjectTypeIds(Optional localName As String = Constants.CmisPredefinedPropertyNames.AllowedChildObjectTypeIds,
                                                Optional queryName As String = Constants.CmisPredefinedPropertyNames.AllowedChildObjectTypeIds) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.AllowedChildObjectTypeIds, localName,
                                     "Id’s of the set of Object-types that can be created, moved or filed into this folder.", queryName,
                                     False, False, Core.enumCardinality.multi, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the baseTypeId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function BaseTypeId(Optional localName As String = Constants.CmisPredefinedPropertyNames.BaseTypeId,
                                 Optional queryName As String = Constants.CmisPredefinedPropertyNames.BaseTypeId,
                                 Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.BaseTypeId, localName,
                                     "Id of the base object-type for the object", queryName,
                                     False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the changeToken-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ChangeToken(Optional localName As String = Constants.CmisPredefinedPropertyNames.ChangeToken,
                                  Optional queryName As String = Constants.CmisPredefinedPropertyNames.ChangeToken) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.ChangeToken, localName,
                                         "Opaque token used for optimistic locking & concurrency checking", queryName,
                                         False, False, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the checkinComment-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CheckinComment(Optional localName As String = Constants.CmisPredefinedPropertyNames.CheckinComment,
                                     Optional queryName As String = Constants.CmisPredefinedPropertyNames.CheckinComment,
                                     Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.CheckinComment, localName,
                                         "Textual comment associated with the given version", queryName,
                                         False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the contentStreamFileName-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ContentStreamFileName(Optional localName As String = Constants.CmisPredefinedPropertyNames.ContentStreamFileName,
                                            Optional queryName As String = Constants.CmisPredefinedPropertyNames.ContentStreamFileName,
                                            Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.ContentStreamFileName, localName,
                                         "File name of the Content Stream", queryName,
                                         False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the contentStreamId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ContentStreamId(Optional localName As String = Constants.CmisPredefinedPropertyNames.ContentStreamId,
                                      Optional queryName As String = Constants.CmisPredefinedPropertyNames.ContentStreamId,
                                      Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.ContentStreamId, localName,
                                     "Id of the stream", queryName,
                                     False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyIntegerDefinition-instance for the contentStreamLength-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ContentStreamLength(Optional localName As String = Constants.CmisPredefinedPropertyNames.ContentStreamLength,
                                          Optional queryName As String = Constants.CmisPredefinedPropertyNames.ContentStreamLength,
                                          Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         'the maximum is limited by the int32 type
         Return PropertyIntegerDefinition(Constants.CmisPredefinedPropertyNames.ContentStreamLength, localName,
                                          "Length of the content stream (in bytes).", queryName,
                                          False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly,
                                          Integer.MaxValue, 0)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the contentStreamMimeType-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ContentStreamMimeType(Optional localName As String = Constants.CmisPredefinedPropertyNames.ContentStreamMimeType,
                                            Optional queryName As String = Constants.CmisPredefinedPropertyNames.ContentStreamMimeType,
                                            Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.ContentStreamMimeType, localName,
                                         "MIME type of the Content Stream", queryName,
                                         False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the createdBy-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreatedBy(Optional localName As String = Constants.CmisPredefinedPropertyNames.CreatedBy,
                                Optional queryName As String = Constants.CmisPredefinedPropertyNames.CreatedBy) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.CreatedBy, localName,
                                         "User who created the object.", queryName,
                                         False, True, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyDateTimeDefinition-instance for the creationDate-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreationDate(Optional localName As String = Constants.CmisPredefinedPropertyNames.CreationDate,
                                   Optional queryName As String = Constants.CmisPredefinedPropertyNames.CreationDate) As ccdp.cmisPropertyDefinitionType
         Return PropertyDateTimeDefinition(Constants.CmisPredefinedPropertyNames.CreationDate, localName,
                                           "DateTime when the object was created.", queryName,
                                           False, True, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the description-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <param name="updatability"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Description(Optional localName As String = Constants.CmisPredefinedPropertyNames.Description,
                                  Optional queryName As String = Constants.CmisPredefinedPropertyNames.Description,
                                  Optional orderable As Boolean = True,
                                  Optional updatability As Core.enumUpdatability = Core.enumUpdatability.readwrite) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.Description, localName,
                                         "Description of the object", queryName,
                                         False, orderable, Core.enumCardinality.single, updatability)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the foreignChangeToken-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ForeignChangeToken(Optional localName As String = Constants.CmisPredefinedPropertyNames.Extensions.ForeignChangeToken,
                                         Optional queryName As String = Constants.CmisPredefinedPropertyNames.Extensions.ForeignChangeToken) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.Extensions.ForeignChangeToken, localName,
                                         "Opaque token used for optimistic locking & concurrency checking in coupled cmis-systems", queryName,
                                         False, False, Core.enumCardinality.single, Core.enumUpdatability.readwrite)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the foreignChangeToken-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ForeignObjectId(Optional localName As String = Constants.CmisPredefinedPropertyNames.Extensions.ForeignObjectId,
                                      Optional queryName As String = Constants.CmisPredefinedPropertyNames.Extensions.ForeignObjectId) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.Extensions.ForeignObjectId, localName,
                                     "ObjectId for an object in an external cmis-system", queryName,
                                     False, False, Core.enumCardinality.single, Core.enumUpdatability.readwrite)
      End Function
      ''' <summary>
      ''' Returns PropertyBooleanDefinition-instance for the isImmutable-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function IsImmutable(Optional localName As String = Constants.CmisPredefinedPropertyNames.IsImmutable,
                                  Optional queryName As String = Constants.CmisPredefinedPropertyNames.IsImmutable,
                                  Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyBooleanDefinition(Constants.CmisPredefinedPropertyNames.IsImmutable, localName,
                                          "TRUE if the repository MUST throw an error at any attempt to update or delete the object.", queryName,
                                          False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyBooleanDefinition-instance for the IsLatestMajorVersion-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function IsLatestMajorVersion(Optional localName As String = Constants.CmisPredefinedPropertyNames.IsLatestMajorVersion,
                                           Optional queryName As String = Constants.CmisPredefinedPropertyNames.IsLatestMajorVersion,
                                           Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyBooleanDefinition(Constants.CmisPredefinedPropertyNames.IsLatestMajorVersion, localName,
                                          "TRUE if the version designated as a major version and is the version, that has the most recent last modiﬁcation date", queryName,
                                          False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyBooleanDefinition-instance for the isLatestVersion-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function IsLatestVersion(Optional localName As String = Constants.CmisPredefinedPropertyNames.IsLatestVersion,
                                      Optional queryName As String = Constants.CmisPredefinedPropertyNames.IsLatestVersion,
                                      Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyBooleanDefinition(Constants.CmisPredefinedPropertyNames.IsLatestVersion, localName,
                                          "TRUE if the version that has the most recent last modiﬁcation date", queryName,
                                          False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyBooleanDefinition-instance for the isMajorVersion-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function IsMajorVersion(Optional localName As String = Constants.CmisPredefinedPropertyNames.IsMajorVersion,
                                     Optional queryName As String = Constants.CmisPredefinedPropertyNames.IsMajorVersion,
                                     Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyBooleanDefinition(Constants.CmisPredefinedPropertyNames.IsMajorVersion, localName,
                                          "TRUE if the version designated as a major version", queryName,
                                          False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyBooleanDefinition-instance for the isPrivateWorkingCopy-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function IsPrivateWorkingCopy(Optional localName As String = Constants.CmisPredefinedPropertyNames.IsPrivateWorkingCopy,
                                           Optional queryName As String = Constants.CmisPredefinedPropertyNames.IsPrivateWorkingCopy,
                                           Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyBooleanDefinition(Constants.CmisPredefinedPropertyNames.IsPrivateWorkingCopy, localName,
                                          "TRUE if the document object is a Private Working Copy", queryName,
                                          False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyBooleanDefinition-instance for the isVersionSeriesCheckedOut-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function IsVersionSeriesCheckedOut(Optional localName As String = Constants.CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut,
                                                Optional queryName As String = Constants.CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut,
                                                Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyBooleanDefinition(Constants.CmisPredefinedPropertyNames.IsVersionSeriesCheckedOut, localName,
                                          "TRUE if there currenly exists a Private Working Copy for this version series", queryName,
                                          False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyDateTimeDefinition-instance for the lastModification-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function LastModificationDate(Optional localName As String = Constants.CmisPredefinedPropertyNames.LastModificationDate,
                                           Optional queryName As String = Constants.CmisPredefinedPropertyNames.LastModificationDate) As ccdp.cmisPropertyDefinitionType
         Return PropertyDateTimeDefinition(Constants.CmisPredefinedPropertyNames.LastModificationDate, localName,
                                           "DateTime when the object was last modified.", queryName,
                                           False, True, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the lastModifiedBy-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function LastModifiedBy(Optional localName As String = Constants.CmisPredefinedPropertyNames.LastModifiedBy,
                                     Optional queryName As String = Constants.CmisPredefinedPropertyNames.LastModifiedBy) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.LastModifiedBy, localName,
                                         "User who last modified the object.", queryName,
                                         False, True, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the name-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName">ignored for base types</param>
      ''' <param name="orderable"></param>
      ''' <param name="updatability"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Name(Optional localName As String = Constants.CmisPredefinedPropertyNames.Name,
                           Optional queryName As String = Constants.CmisPredefinedPropertyNames.Name,
                           Optional orderable As Boolean = True,
                           Optional updatability As Core.enumUpdatability = Core.enumUpdatability.readwrite) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.Name, localName,
                                         "Name of the object", queryName,
                                         True, orderable, Core.enumCardinality.single, updatability)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the objectId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ObjectId(Optional localName As String = Constants.CmisPredefinedPropertyNames.ObjectId,
                               Optional queryName As String = Constants.CmisPredefinedPropertyNames.ObjectId,
                               Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.ObjectId, localName,
                                     "Id of the object", queryName,
                                     False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the objectTypeId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ObjectTypeId(Optional localName As String = Constants.CmisPredefinedPropertyNames.ObjectTypeId,
                                   Optional queryName As String = Constants.CmisPredefinedPropertyNames.ObjectTypeId,
                                   Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.ObjectTypeId, localName,
                                     "Id of the object’s type", queryName,
                                     True, orderable, Core.enumCardinality.single, Core.enumUpdatability.oncreate)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the parentId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function ParentId(Optional localName As String = Constants.CmisPredefinedPropertyNames.ParentId,
                               Optional queryName As String = Constants.CmisPredefinedPropertyNames.ParentId) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.ParentId, localName,
                                     "ID of the parent folder of the folder.", queryName,
                                     False, False, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the path-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function Path(Optional localName As String = Constants.CmisPredefinedPropertyNames.Path,
                           Optional queryName As String = Constants.CmisPredefinedPropertyNames.Path,
                           Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.Path, localName,
                                         "The fully qualified path to this folder.", queryName,
                                         False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the policyText-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function PolicyText(Optional localName As String = Constants.CmisPredefinedPropertyNames.PolicyText,
                                 Optional queryName As String = Constants.CmisPredefinedPropertyNames.PolicyText,
                                 Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.PolicyText, localName,
                                         "User-friendly description of the policy", queryName,
                                         False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns the PropertyDateTimeDefinition-instance for the rm_destructionDate-property
      ''' </summary>
      Public Function RM_DestructionDate(orderable As Boolean, queryable As Boolean,
                                         Optional localName As String = Constants.CmisPredefinedPropertyNames.RM_DestructionDate,
                                         Optional queryName As String = Constants.CmisPredefinedPropertyNames.RM_DestructionDate) As ccdp.cmisPropertyDefinitionType
         Dim retVal As ccdp.cmisPropertyDefinitionType = PropertyDateTimeDefinition(Constants.CmisPredefinedPropertyNames.RM_DestructionDate, localName,
                                                                                    "Destruction date", queryName, False, orderable,
                                                                                    Core.enumCardinality.single, Core.enumUpdatability.readwrite)
         retVal.Queryable = queryable
         Return retVal
      End Function
      ''' <summary>
      ''' Returns the PropertyDateTimeDefinition-instance for the rm_expirationDate-property
      ''' </summary>
      Public Function RM_ExpirationDate(orderable As Boolean,
                                        Optional localName As String = Constants.CmisPredefinedPropertyNames.RM_ExpirationDate,
                                        Optional queryName As String = Constants.CmisPredefinedPropertyNames.RM_ExpirationDate) As ccdp.cmisPropertyDefinitionType
         Return PropertyDateTimeDefinition(Constants.CmisPredefinedPropertyNames.RM_ExpirationDate, localName,
                                           "Expiration date", queryName, False, orderable,
                                           Core.enumCardinality.single, Core.enumUpdatability.readwrite)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the rm_holdIds-property
      ''' </summary>
      Public Function RM_HoldIds(Optional localName As String = Constants.CmisPredefinedPropertyNames.RM_HoldIds,
                                 Optional queryName As String = Constants.CmisPredefinedPropertyNames.RM_HoldIds,
                                 Optional queryable As Boolean = True,
                                 Optional updatability As Core.enumUpdatability = Core.enumUpdatability.readwrite) As ccdp.cmisPropertyDefinitionType
         Dim retVal As ccdp.cmisPropertyDefinitionType = PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.RM_HoldIds, localName,
                                                                                  "Hold identifiers", queryName, False, False,
                                                                                  Core.enumCardinality.multi, updatability)
         retVal.Queryable = queryable
         Return retVal
      End Function
      ''' <summary>
      ''' Returns PropertyDateTimeDefinition-instance for the rm_startOfRetention-property
      ''' </summary>
      Public Function RM_StartOfRetention(orderable As Boolean, queryable As Boolean,
                                          Optional localName As String = Constants.CmisPredefinedPropertyNames.RM_StartOfRetention,
                                          Optional queryName As String = Constants.CmisPredefinedPropertyNames.RM_StartOfRetention,
                                          Optional updatability As Core.enumUpdatability = Core.enumUpdatability.readwrite) As ccdp.cmisPropertyDefinitionType
         Dim retVal As ccdp.cmisPropertyDefinitionType = PropertyDateTimeDefinition(Constants.CmisPredefinedPropertyNames.RM_StartOfRetention, localName,
                                                                                    "Start of retention", queryName, False, orderable,
                                                                                    Core.enumCardinality.single, updatability)
         retVal.Queryable = queryable
         Return retVal
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the secondaryObjectTypeIds-property
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function SecondaryObjectTypeIds(Optional localName As String = Constants.CmisPredefinedPropertyNames.SecondaryObjectTypeIds,
                                             Optional queryName As String = Constants.CmisPredefinedPropertyNames.SecondaryObjectTypeIds,
                                             Optional updatability As Core.enumUpdatability = Core.enumUpdatability.readonly) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.SecondaryObjectTypeIds, localName,
                                     "Ids of the object’s secondary types", queryName,
                                     False, False, Core.enumCardinality.multi, updatability)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the sourceId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function SourceId(Optional localName As String = Constants.CmisPredefinedPropertyNames.SourceId,
                               Optional queryName As String = Constants.CmisPredefinedPropertyNames.SourceId,
                               Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.SourceId, localName,
                                     "ID of the source object of the relationship.", queryName,
                                     False, orderable, Core.enumCardinality.single, Core.enumUpdatability.oncreate)
      End Function
      ''' <summary>
      ''' Returns PropertyIntegerDefinition-instance for the syncRequired-property
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function SyncRequired(Optional localName As String = Constants.CmisPredefinedPropertyNames.Extensions.SyncRequired,
                                   Optional queryName As String = Constants.CmisPredefinedPropertyNames.Extensions.SyncRequired,
                                   Optional maxValue As xs_Integer = enumSyncRequired.suspendedBiDirectional,
                                   Optional minValue As xs_Integer = enumSyncRequired.custom) As ccdp.cmisPropertyDefinitionType
         Return PropertyIntegerDefinition(Constants.CmisPredefinedPropertyNames.Extensions.SyncRequired, localName,
                                          "Pending synchronization with an external cmis-system", queryName,
                                          False, False, Core.enumCardinality.single, Core.enumUpdatability.readwrite, maxValue, minValue)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the targetId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function TargetId(Optional localName As String = Constants.CmisPredefinedPropertyNames.TargetId,
                               Optional queryName As String = Constants.CmisPredefinedPropertyNames.TargetId,
                               Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.TargetId, localName,
                                     "ID of the target object of the relationship.", queryName,
                                     False, orderable, Core.enumCardinality.single, Core.enumUpdatability.oncreate)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the versionLabel-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function VersionLabel(Optional localName As String = Constants.CmisPredefinedPropertyNames.VersionLabel,
                                   Optional queryName As String = Constants.CmisPredefinedPropertyNames.VersionLabel,
                                   Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.VersionLabel, localName,
                                         "Textual description the position of an individual object with respect to the version series", queryName,
                                         False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyStringDefinition-instance for the versionSeriesCheckedOutBy-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function VersionSeriesCheckedOutBy(Optional localName As String = Constants.CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy,
                                                Optional queryName As String = Constants.CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy,
                                                Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyStringDefinition(Constants.CmisPredefinedPropertyNames.VersionSeriesCheckedOutBy, localName,
                                         "An identiﬁer for the user who created the Private Working Copy", queryName,
                                         False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the versionSeriesCheckedOutId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function VersionSeriesCheckedOutId(Optional localName As String = Constants.CmisPredefinedPropertyNames.VersionSeriesCheckedOutId,
                                                Optional queryName As String = Constants.CmisPredefinedPropertyNames.VersionSeriesCheckedOutId,
                                                Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.VersionSeriesCheckedOutId, localName,
                                     "The object id for the Private Working Copy or ""not set""", queryName,
                                     False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
      ''' <summary>
      ''' Returns PropertyIdDefinition-instance for the versionSeriesId-property
      ''' </summary>
      ''' <param name="localName"></param>
      ''' <param name="queryName"></param>
      ''' <param name="orderable"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function VersionSeriesId(Optional localName As String = Constants.CmisPredefinedPropertyNames.VersionSeriesId,
                                      Optional queryName As String = Constants.CmisPredefinedPropertyNames.VersionSeriesId,
                                      Optional orderable As Boolean = True) As ccdp.cmisPropertyDefinitionType
         Return PropertyIdDefinition(Constants.CmisPredefinedPropertyNames.VersionSeriesId, localName,
                                     "Id of the version series for this object.", queryName,
                                     False, orderable, Core.enumCardinality.single, Core.enumUpdatability.readonly)
      End Function
   End Class
End Namespace