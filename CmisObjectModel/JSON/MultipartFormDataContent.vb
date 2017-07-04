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
Imports ccs = CmisObjectModel.Core.Security
Imports sn = System.Net
Imports sw = System.Web

'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.JSON
   ''' <summary>
   ''' Class handles Multipart-content in browser binding
   ''' </summary>
   ''' <remarks></remarks>
   Public Class MultipartFormDataContent
      Inherits HttpContent

#Region "Constructors"
      Public Sub New(contentType As String)
         MyBase.New(Nothing)

         If Not String.IsNullOrEmpty(contentType) Then
            Dim match As System.Text.RegularExpressions.Match = _regExContentType.Match(contentType)

            If match IsNot Nothing AndAlso match.Success Then
               Dim grBoundary As System.Text.RegularExpressions.Group = match.Groups("Boundary")
               Dim grUrlEncoded As System.Text.RegularExpressions.Group = match.Groups("UrlEncoded")
               Dim grCharset As System.Text.RegularExpressions.Group = match.Groups(charsetGroupName)

               IsUrlEncoded = (grUrlEncoded IsNot Nothing AndAlso grUrlEncoded.Success)
               Boundary = If(grBoundary IsNot Nothing AndAlso grBoundary.Success, grBoundary.Value, If(IsUrlEncoded, String.Empty, System.Guid.NewGuid.ToString()))
               If grCharset IsNot Nothing AndAlso grCharset.Success Then
                  Try
                     UrlEncoding = System.Text.Encoding.GetEncoding(grCharset.Value)
                  Catch
                  End Try
               End If
               Me.ContentType = contentType
            End If
         End If
      End Sub

      Public Sub New(contentType As String, cmisAction As String)
         Me.New(ProcessContentType(contentType, cmisAction))
         Me.Add("cmisaction", cmisAction)
      End Sub

      Public Sub New(contentType As String, cmisAction As String, request As CmisObjectModel.Messaging.Requests.RequestBase)
         Me.New(contentType, cmisAction)
         If request.BrowserBinding.Succinct Then Me.Add("succinct", Convert(request.BrowserBinding.Succinct))
      End Sub

      Private Shared Function ProcessContentType(contentType As String, cmisAction As String) As String
         If String.Compare(contentType, Constants.MediaTypes.MultipartFormData, True) = 0 Then
            Dim affix As String = "CmisObjectModel" & System.Guid.NewGuid.ToString("N")
            Return contentType & "; boundary=" & affix & cmisAction & affix
         Else
            Return contentType
         End If
      End Function
#End Region

#Region "Helper-classes"
      ''' <summary>
      ''' A simple multi dimensional matrix
      ''' </summary>
      ''' <remarks></remarks>
      Public Class Matrix(Of T)
         Inherits List(Of Matrix(Of T))

         ''' <summary>
         ''' Encapsulates value with a Matrix(Of T) instance and puts it to the end of the list
         ''' </summary>
         Public Overloads Function Add(value As T) As Matrix(Of T)
            Dim retVal As New Matrix(Of T) With {._value = value}

            MyBase.Add(retVal)
            Return retVal
         End Function

         ''' <summary>
         ''' Encapsulates values with Matrix(Of T) instances and puts them to the end of the list
         ''' </summary>
         ''' <param name="values"></param>
         ''' <remarks></remarks>
         Public Overloads Sub AddRange(values As IEnumerable(Of T))
            If values IsNot Nothing Then
               MyBase.AddRange(From value As T In values
                               Select New Matrix(Of T) With {._value = value})
            End If
         End Sub

         ''' <summary>
         ''' Ensures a valid item at given position
         ''' </summary>
         Private Function GetSafeItem(index As Integer) As Matrix(Of T)
            Dim retVal As Matrix(Of T)

            If Count <= index Then
               MyBase.AddRange(CType(Array.CreateInstance(GetType(Matrix(Of T)), 1 + index - Count), Matrix(Of T)()))
            End If
            retVal = MyBase.Item(index)
            If retVal Is Nothing Then
               retVal = New Matrix(Of T)
               MyBase.Item(index) = retVal
            End If

            Return retVal
         End Function

         ''' <summary>
         ''' Returns the values of all items
         ''' </summary>
         Public ReadOnly Property ItemsValues(ParamArray indexes As Integer()) As T()
            Get
               Dim instance As Matrix(Of T) = Me

               If indexes IsNot Nothing Then
                  For Each index As Integer In indexes
                     instance = instance.GetSafeItem(index)
                  Next
               End If

               If instance.Count = 0 Then
                  Return Nothing
               Else
                  Return (From matrix As Matrix(Of T) In instance
                          Let value As T = If(matrix Is Nothing, Nothing, matrix._value)
                          Select value).ToArray()
               End If
            End Get
         End Property

         Private _value As T
         Public Property Value(ParamArray indexes As Integer()) As T
            Get
               Dim instance As Matrix(Of T) = Me

               If indexes IsNot Nothing Then
                  For Each index As Integer In indexes
                     instance = instance.GetSafeItem(index)
                  Next
               End If
               Return instance._value
            End Get
            Set(value As T)
               Dim instance As Matrix(Of T) = Me

               If indexes IsNot Nothing Then
                  For Each index As Integer In indexes
                     instance = instance.GetSafeItem(index)
                  Next
               End If
               instance._value = value
            End Set
         End Property
      End Class
#End Region

      Private _aces As Dictionary(Of enumCollectionAction, Matrix(Of String))
      Private _aclPropagation As CmisObjectModel.Core.enumACLPropagation?

      ''' <summary>
      ''' Adds another content
      ''' </summary>
      ''' <param name="value"></param>
      ''' <remarks></remarks>
      Public Function Add(value As HttpContent) As Exception
         Dim key As String = If(value Is Nothing, Nothing, value.ContentDisposition)
         Dim lowerKey As String = If(key, String.Empty).ToLowerInvariant()

         If value Is Nothing Then
            Return New ArgumentNullException("value", "Parameter MUST NOT be null.")
         ElseIf String.IsNullOrEmpty(key) Then
            Return New ArgumentException("Headers", "value.Headers MUST contain '" & Common.RFC2231Helper.ContentDispositionHeaderName & "' or the value MUST NOT be null or empty.")
         ElseIf _contentMap.ContainsKey(lowerKey) Then
            Return New ArgumentException("Content '" & key & "' already exists.")
         Else
            _contents.Add(value)
            _contentMap.Add(lowerKey, value)
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Adds a simple parameter
      ''' </summary>
      Public Function Add(parameterName As String, parameterValue As String) As Exception
         Dim content As New HttpContent(If(String.IsNullOrEmpty(parameterValue), New Byte() {}, System.Text.Encoding.UTF8.GetBytes(parameterValue)))

         content.Headers.Add(Common.RFC2231Helper.ContentDispositionHeaderName, "form-data; name=""" & parameterName & """")
         content.Headers.Add(Common.RFC2231Helper.ContentTypeHeaderName, Constants.MediaTypes.PlainText & "; charset=utf-8")
         Return Add(content)
      End Function

      ''' <summary>
      ''' Adds extensions of cmisPropertiesType
      ''' </summary>
      Public Sub Add(propertiesExtensions As CmisObjectModel.Core.cmisObjectType.PropertiesExtensions)
         Dim serializer As New Serialization.JavaScriptSerializer
         Add("propertiesExtension", serializer.Serialize(propertiesExtensions))
      End Sub

      ''' <summary>
      ''' Adds a property
      ''' </summary>
      ''' <remarks>Follows "5.4.4.3.11 Single-value Properties" and "5.4.4.3.12 Multi-value Properties"</remarks>
      Public Sub Add([property] As CmisObjectModel.Core.Properties.cmisProperty)
         If _properties Is Nothing Then _properties = New Matrix(Of String)

         Dim propertyDefinitionId As String = [property].PropertyDefinitionId
         Dim index As Integer = _properties.Count
         Dim propertyType As Type = [property].PropertyType
         'JSON transfers date properties as longs
         Dim grh As Common.GenericRuntimeHelper =
            Common.GenericRuntimeHelper.GetInstance(GetJSONType(propertyType))
         Dim fnConvert As Func(Of Object, String) =
            Function(value)
               If TypeOf value Is DateTimeOffset Then
                  value = CType(value, DateTimeOffset).DateTime.ToJSONTime()
               ElseIf TypeOf value Is DateTimeOffset? Then
                  With CType(value, DateTimeOffset?)
                     If .HasValue Then
                        value = .Value.DateTime.ToJSONTime()
                     Else
                        value = CType(Nothing, Long?)
                     End If
                  End With
               ElseIf TypeOf value Is DateTime Then
                  value = CType(value, DateTime).ToJSONTime()
               ElseIf TypeOf value Is DateTime? Then
                  With CType(value, DateTime?)
                     If .HasValue Then
                        value = .Value.ToJSONTime()
                     Else
                        value = CType(Nothing, Long?)
                     End If
                  End With
               End If
               If Not (value Is Nothing OrElse TypeOf value Is String) Then value = grh.Convert(value)

               Return CStr(value)
            End Function
         Dim values As List(Of String) = If([property].Values Is Nothing, Nothing,
                                            (From value As Object In [property].Values
                                             Select fnConvert(value)).ToList())

         _properties.Add(propertyDefinitionId).AddRange(values)
         Add("propertyId[" & index & "]", propertyDefinitionId)
         If values IsNot Nothing Then
            Dim prefix As String = "propertyValue[" & index & "]"

            If [property].Cardinality = CmisObjectModel.Core.enumCardinality.multi OrElse values.Count > 1 Then
               For subIndex As Integer = 0 To values.Count - 1
                  Add(prefix & "[" & subIndex & "]", values(subIndex))
               Next
            ElseIf values.Count = 1 Then
               Add(prefix, values(0))
            End If
         End If
      End Sub

      ''' <summary>
      ''' Adds a typedefinition
      ''' </summary>
      ''' <param name="type"></param>
      ''' <remarks>chapter 5.4.4.3.31 Type in cmis documentation</remarks>
      Public Sub Add(type As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType)
         Dim serializer As New Serialization.JavaScriptSerializer
         Add("type", serializer.Serialize(type))
      End Sub

      ''' <summary>
      ''' Adds information about a secondary type id
      ''' </summary>
      Public Sub Add(secondaryTypeId As String, action As enumCollectionAction)
         Dim secondaryTypeIds As List(Of String)

         If _secondaryTypeIds Is Nothing Then _secondaryTypeIds = New Dictionary(Of enumCollectionAction, List(Of String))
         secondaryTypeIds = GetSafeDictionaryValue(_secondaryTypeIds, action)
         Add(action.GetName() & "SecondaryTypeId[" & secondaryTypeIds.Count & "]", secondaryTypeId)
         secondaryTypeIds.Add(secondaryTypeId)
      End Sub

      ''' <summary>
      ''' Adds information about an accessControlEntry
      ''' </summary>
      Public Sub Add(ace As CmisObjectModel.Core.Security.cmisAccessControlEntryType, action As enumCollectionAction)
         If ace IsNot Nothing AndAlso ace.Principal IsNot Nothing Then


            If _aces Is Nothing Then _aces = New Dictionary(Of enumCollectionAction, Matrix(Of String))

            Dim aces As Matrix(Of String) = GetSafeDictionaryValue(_aces, action)
            Dim prefix As String = action.GetName() & "ACEPermission[" & aces.Count & "]"
            Dim permissions As String() = If(ace.Permissions, New String() {})

            Add(action.GetName() & "ACEPrincipal[" & aces.Count & "]", ace.Principal.PrincipalId)
            For index As Integer = 0 To permissions.Length
               Add(prefix & "[" & index & "]", permissions(index))
            Next
            aces.Add(ace.Principal.PrincipalId).AddRange(permissions)
         End If
      End Sub

      ''' <summary>
      ''' Adds aclPropagation information
      ''' </summary>
      ''' <param name="propagation"></param>
      ''' <remarks></remarks>
      Public Sub Add(propagation As CmisObjectModel.Core.enumACLPropagation)
         _aclPropagation = propagation
         Add("ACLPropagation", propagation.GetName())
      End Sub

      ''' <summary>
      ''' Adds autoindexed value-information
      ''' </summary>
      Public Sub Add(value As String, type As enumValueType)
         Dim autoIndexedValues As List(Of String)

         If _autoIndexedValues Is Nothing Then _autoIndexedValues = New Dictionary(Of enumValueType, List(Of String))
         autoIndexedValues = GetSafeDictionaryValue(_autoIndexedValues, type)
         Add(type.GetName() & "[" & autoIndexedValues.Count & "]", value)
         autoIndexedValues.Add(value)
      End Sub

      Private _autoIndexedValues As Dictionary(Of enumValueType, List(Of String))
      Public ReadOnly Boundary As String

      ''' <summary>
      ''' List of embedded contents
      ''' </summary>
      ''' <remarks></remarks>
      Private ReadOnly _contents As New List(Of HttpContent)
      Private ReadOnly _contentMap As New Dictionary(Of String, HttpContent)
      Public ReadOnly Property Content(key As String) As HttpContent
         Get
            If key Is Nothing Then
               Return Nothing
            Else
               key = key.ToLowerInvariant()
               Return If(_contentMap.ContainsKey(key), _contentMap(key), Nothing)
            End If
         End Get
      End Property
      Public ReadOnly Property Contents As HttpContent()
         Get
            Return _contents.ToArray()
         End Get
      End Property

      ''' <summary>
      ''' Searches all contents for aces, aclPropagation, autoIndexed values, properties and secondary type ids
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub ExtractAll()
         Dim regEx As New System.Text.RegularExpressions.Regex("((?<action>\S+)(?<Type>(ACEPrincipal|SecondaryTypeId))\[(?<Index>\d+)\]" &
                                                               "|(?<action>\S+)(?<Type>ACEPermission)\[(?<Index>\d+)\]\[(?<SubIndex>\d+)\]" &
                                                               "|(?<Type>[^\[]+)\[(?<Index>\d+)\](\[(?<SubIndex>\d+)\])?" &
                                                               "|(?<Type>(ACLPropagation|PropertiesExtension|Type))" &
                                                               ")",
                                                               Text.RegularExpressions.RegexOptions.ExplicitCapture Or
                                                               Text.RegularExpressions.RegexOptions.IgnoreCase Or
                                                               Text.RegularExpressions.RegexOptions.Singleline)
         _aces = New Dictionary(Of enumCollectionAction, Matrix(Of String))
         _autoIndexedValues = New Dictionary(Of enumValueType, List(Of String))
         _properties = New Matrix(Of String)
         _secondaryTypeIds = New Dictionary(Of enumCollectionAction, List(Of String))
         _typeDefinition = String.Empty

         For Each de As KeyValuePair(Of String, HttpContent) In _contentMap
            Dim match As System.Text.RegularExpressions.Match = regEx.Match(de.Key)

            If match IsNot Nothing AndAlso match.Success Then
               Dim action As enumCollectionAction = enumCollectionAction.add
               Dim index As Integer = 0
               Dim subIndex As Integer = 0
               Dim group As System.Text.RegularExpressions.Group
               Dim value As String = de.Value.ToString()

               group = match.Groups("action")
               If group IsNot Nothing AndAlso group.Success Then TryParse(group.Value, action, True)
               group = match.Groups("Index")
               If group IsNot Nothing AndAlso group.Success Then index = CInt(group.Value)
               group = match.Groups("SubIndex")
               If group IsNot Nothing AndAlso group.Success Then subIndex = CInt(group.Value)

               Select Case match.Groups("Type").Value.ToLowerInvariant()
                  Case "aceprincipal"
                     GetSafeDictionaryValue(_aces, action).Value(index) = value
                  Case "acepermission"
                     GetSafeDictionaryValue(_aces, action).Value(index, subIndex) = value
                  Case "aclpropagation"
                     _aclPropagation = ParseEnum(Of CmisObjectModel.Core.enumACLPropagation)(value)
                  Case "propertiesextension"
                     _propertiesExtensions = If(value, String.Empty)
                  Case "propertyid"
                     _properties.Value(index) = value
                  Case "propertyvalue"
                     _properties.Value(index, subIndex) = value
                  Case "secondarytypeid"
                     SetAt(GetSafeDictionaryValue(_secondaryTypeIds, action), value, index)
                  Case "type"
                     _typeDefinition = If(value, String.Empty)
                  Case Else
                     'auto indexed values
                     Dim valueType As enumValueType
                     If TryParse(match.Groups("Type").Value, valueType, True) Then
                        SetAt(GetSafeDictionaryValue(_autoIndexedValues, valueType), value, index)
                     End If
               End Select
            End If
         Next
      End Sub

      ''' <summary>
      ''' Extracts multipart content from stream
      ''' </summary>
      ''' <param name="stream"></param>
      ''' <param name="contentType"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function FromStream(stream As IO.Stream, contentType As String) As MultipartFormDataContent
         Dim retVal As New MultipartFormDataContent(contentType)

         If Not (stream Is Nothing OrElse String.IsNullOrEmpty(contentType)) Then
            If retVal.IsUrlEncoded Then
               'simple format: urlencoded
               Using ms As New IO.MemoryStream()
                  stream.CopyTo(ms)
                  ms.Position = 0
                  Dim nvc As System.Collections.Specialized.NameValueCollection = sw.HttpUtility.ParseQueryString(retVal.UrlEncoding.GetString(ms.ToArray()))
                  For Each name As String In nvc
                     retVal.Add(name, nvc(name))
                  Next
                  ms.Close()
               End Using
            Else
               'multipart/form-data
               '8-bit codepage (may interpret utf-8 chars wrongly)
               Dim cp850 As System.Text.Encoding = System.Text.Encoding.GetEncoding(850)
               'ensure same "mis-interpretation"
               Dim boundary As String = cp850.GetString(System.Text.Encoding.UTF8.GetBytes("--" & retVal.Boundary))
               Const newLinePattern As String = "(\r\n|\n)"
               Const headersPattern As String = "((?<headerId>[^:\r\n]+):\s?(?<headerValue>[^\r\n]+)" & newLinePattern & ")+"
               Const valuePattern As String = "(?<value>[\s\S]*?)"
               Dim boundaryPattern As String = Common.CreateRegExPattern(boundary)
               Dim pattern As String = "(?<=" & boundaryPattern & newLinePattern & ")" & headersPattern & newLinePattern & valuePattern & "(?=(" & newLinePattern & ")?" & boundaryPattern & ")"
               Dim data As Byte()
               Dim regEx As New System.Text.RegularExpressions.Regex(pattern, Text.RegularExpressions.RegexOptions.ExplicitCapture Or Text.RegularExpressions.RegexOptions.Multiline)

               Using ms As New IO.MemoryStream()
                  stream.CopyTo(ms)
                  ms.Position = 0
                  data = ms.ToArray()
                  Dim content As String = cp850.GetString(data)

                  For Each match As System.Text.RegularExpressions.Match In regEx.Matches(content)
                     Dim value As Byte() = CType(Array.CreateInstance(GetType(Byte), match.Groups("value").Length), Byte())
                     Dim httpContent As New JSON.HttpContent(value)
                     Dim headerIds As System.Text.RegularExpressions.CaptureCollection = match.Groups("headerId").Captures
                     Dim headerValues As System.Text.RegularExpressions.CaptureCollection = match.Groups("headerValue").Captures

                     If value.Length > 0 Then Array.Copy(data, match.Groups("value").Index, value, 0, value.Length)
                     For index As Integer = 0 To Math.Min(headerIds.Count, headerValues.Count) - 1
                        Dim headerId As String = System.Text.Encoding.UTF8.GetString(data, headerIds(index).Index, headerIds(index).Length)
                        Dim headerValue As String = System.Text.Encoding.UTF8.GetString(data, headerValues(index).Index, headerValues(index).Length)
                        httpContent.Headers(headerId) = headerValue
                     Next
                     retVal.Add(httpContent)
                  Next
                  ms.Close()
               End Using
            End If
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Returns the aces (add or remove) stored in this instance
      ''' </summary>
      Public Function GetACEs(action As enumCollectionAction) As ccs.cmisAccessControlListType
         'evaluate HttpContents
         If _aces Is Nothing Then ExtractAll()
         If _aces.ContainsKey(action) Then
            Return New ccs.cmisAccessControlListType() With {
               .Permissions = (From ace As Matrix(Of String) In _aces(action)
                               Let principal As ccs.cmisAccessControlPrincipalType = New ccs.cmisAccessControlPrincipalType() With {.PrincipalId = ace.Value}
                               Let permissions As String() = ace.ItemsValues
                               Select New ccs.cmisAccessControlEntryType() With {.Principal = principal, .Permissions = permissions}).ToArray()}
         Else
            Return Nothing
         End If
      End Function


      ''' <summary>
      ''' Returns the aclPropagation stored in this instance
      ''' </summary>
      Public Function GetACLPropagation() As CmisObjectModel.Core.enumACLPropagation?
         'evaluate HttpContents (an empty _autoIndexedValues signals ExtractAll() has not been called at this moment)
         If _autoIndexedValues Is Nothing Then ExtractAll()
         Return _aclPropagation
      End Function

      ''' <summary>
      ''' Returns auto indexed values like changeTokens or policies stored in this instance
      ''' </summary>
      Public Function GetAutoIndexedValues(type As enumValueType) As String()
         'evaluate HttpContents
         If _autoIndexedValues Is Nothing Then ExtractAll()
         If _autoIndexedValues.ContainsKey(type) Then
            Dim autoIndexedValues As List(Of String) = _autoIndexedValues(type)
            Return If(autoIndexedValues Is Nothing, Nothing, autoIndexedValues.ToArray())
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Returns the properties stored in this instance
      ''' </summary>
      ''' <remarks></remarks>
      Public Function GetProperties(fnGetTypeDefinition As Func(Of String, CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType),
                                    ParamArray typeDefinitions As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType()) As CmisObjectModel.Core.Collections.cmisPropertiesType
         With New Dictionary(Of String, CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType)
            Dim values As String()
            Dim properties As New Dictionary(Of String, String())

            'evaluate HttpContents
            If _properties Is Nothing Then ExtractAll()
            'create an index for quick property access
            For Each matrix As Matrix(Of String) In _properties
               If Not properties.ContainsKey(matrix.Value) Then properties.Add(matrix.Value, matrix.ItemsValues)
            Next
            'object type id and secondary type ids
            For Each propertyName As String In New String() {Constants.CmisPredefinedPropertyNames.ObjectTypeId, Constants.CmisPredefinedPropertyNames.SecondaryObjectTypeIds}
               If properties.ContainsKey(propertyName) Then
                  values = properties(propertyName)
                  If values IsNot Nothing Then
                     For Each value As String In values
                        If Not (String.IsNullOrEmpty(value) OrElse .ContainsKey(value)) Then
                           Dim typeDefinition As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType = fnGetTypeDefinition(value)
                           If typeDefinition IsNot Nothing Then .Add(value, typeDefinition)
                        End If
                     Next
                  End If
               End If
            Next
            'append typedefinitions
            If typeDefinitions IsNot Nothing Then
               For Each td As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType In typeDefinitions
                  If Not (td Is Nothing OrElse .ContainsKey(td.Id)) Then .Add(td.Id, td)
               Next
            End If

            Return GetProperties(.Values.ToArray())
         End With
      End Function

      ''' <summary>
      ''' Returns the properties stored in this instance
      ''' </summary>
      ''' <remarks>Overload object changed</remarks>
      Private Function GetProperties(ParamArray typeDefinitions As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType()) As CmisObjectModel.Core.Collections.cmisPropertiesType
         Dim properties As New List(Of CmisObjectModel.Core.Properties.cmisProperty)
         Dim extensions As CmisObjectModel.Extensions.Extension() = Nothing
         Dim propertyDefinitions As New Dictionary(Of String, CmisObjectModel.Core.Definitions.Properties.cmisPropertyDefinitionType)

         'evaluate HttpContents
         If _properties Is Nothing Then ExtractAll()
         If typeDefinitions IsNot Nothing Then
            For Each td As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType In typeDefinitions
               For Each de As KeyValuePair(Of String, CmisObjectModel.Core.Definitions.Properties.cmisPropertyDefinitionType) In td.GetPropertyDefinitions(enumKeySyntax.lowerCase)
                  If Not propertyDefinitions.ContainsKey(de.Key) Then propertyDefinitions.Add(de.Key, de.Value)
               Next
            Next
         End If
         For Each matrix As Matrix(Of String) In _properties
            Dim key As String = matrix.Value.ToLowerInvariant()
            Dim cmisProperty As CmisObjectModel.Core.Properties.cmisProperty =
               If(propertyDefinitions.ContainsKey(key), propertyDefinitions(key).CreateProperty(),
                  New CmisObjectModel.Core.Properties.cmisPropertyString())
            Dim propertyType As Type = cmisProperty.PropertyType
            'JSON transfers date properties as longs
            Dim grh As Common.GenericRuntimeHelper =
               Common.GenericRuntimeHelper.GetInstance(GetJSONType(propertyType))
            Dim fnConvert As Func(Of String, Object) =
               If(propertyType Is GetType(String),
                  New Func(Of String, Object)(Function(value) value),
                  New Func(Of String, Object)(Function(value)
                                                 Dim retVal As Object = grh.ConvertBack(value, Nothing)

                                                 If propertyType Is GetType(DateTimeOffset) OrElse propertyType Is GetType(DateTime) Then
                                                    Dim dateValue As DateTime = If(TypeOf retVal Is Long, CType(retVal, Long).FromJSONTime(), Nothing)

                                                    If propertyType Is GetType(DateTimeOffset) Then
                                                       retVal = CType(dateValue, DateTimeOffset)
                                                    Else
                                                       retVal = dateValue
                                                    End If
                                                 ElseIf propertyType Is GetType(DateTimeOffset?) OrElse propertyType Is GetType(DateTime?) Then
                                                    Dim dateValue As DateTime?

                                                    If TypeOf retVal Is Long? Then
                                                       With CType(retVal, Long?)
                                                          If .HasValue Then
                                                             dateValue = .Value.FromJSONTime()
                                                          Else
                                                             dateValue = Nothing
                                                          End If
                                                       End With
                                                    End If
                                                    If propertyType Is GetType(DateTime?) Then
                                                       retVal = dateValue
                                                    ElseIf dateValue.HasValue Then
                                                       retVal = CType(dateValue.Value, DateTimeOffset?)
                                                    Else
                                                       retVal = CType(Nothing, DateTimeOffset?)
                                                    End If
                                                 End If

                                                 Return retVal
                                              End Function))

            'property values
            If matrix.Count > 0 Then
               cmisProperty.Values = (From value As String In matrix.ItemsValues
                                      Select fnConvert(value)).ToArray()
            End If
            properties.Add(cmisProperty)
         Next
         If Not String.IsNullOrEmpty(_propertiesExtensions) Then
            Dim serializer As New Serialization.JavaScriptSerializer()
            Dim propertiesExtensions As CmisObjectModel.Core.cmisObjectType.PropertiesExtensions =
               serializer.Deserialize(Of CmisObjectModel.Core.cmisObjectType.PropertiesExtensions)(_propertiesExtensions)
            If propertiesExtensions IsNot Nothing Then extensions = propertiesExtensions.Extensions
         End If

         Return New CmisObjectModel.Core.Collections.cmisPropertiesType(properties.ToArray()) With {.Extensions = extensions}
      End Function

      ''' <summary>
      ''' Gets or creates a valid value for key
      ''' </summary>
      Private Function GetSafeDictionaryValue(Of TKey, TValue As New)(dictionary As Dictionary(Of TKey, TValue), key As TKey) As TValue
         If dictionary.ContainsKey(key) Then
            Return dictionary(key)
         Else
            Dim retVal As New TValue()
            dictionary.Add(key, retVal)
            Return retVal
         End If
      End Function

      ''' <summary>
      ''' Returns the secondaryTypeIds (add or remove) stored in this instance
      ''' </summary>
      Public Function GetSecondaryTypeIds(action As enumCollectionAction) As String()
         'evaluate HttpContents
         If _secondaryTypeIds Is Nothing Then ExtractAll()
         If _secondaryTypeIds.ContainsKey(action) Then
            Dim secondaryTypeIds As List(Of String) = _secondaryTypeIds(action)
            Return If(secondaryTypeIds Is Nothing, Nothing, secondaryTypeIds.ToArray())
         Else
            Return Nothing
         End If
      End Function

      ''' <summary>
      ''' Returns the type definition stored in this instance
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks>chapter 5.4.4.3.31 Type in cmis documentation</remarks>
      Public Function GetTypeDefinition() As CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType
         If _typeDefinition Is Nothing Then ExtractAll()
         If Not String.IsNullOrEmpty(_typeDefinition) Then
            Dim serializer As New Serialization.JavaScriptSerializer()
            Return serializer.Deserialize(Of CmisObjectModel.Core.Definitions.Types.cmisTypeDefinitionType)(_typeDefinition)
         Else
            Return Nothing
         End If
      End Function

      Public ReadOnly IsUrlEncoded As Boolean
      Private _properties As Matrix(Of String)
      Private _propertiesExtensions As String
      Private Shared _regExContentType As New System.Text.RegularExpressions.Regex("(" & Common.CreateRegExPattern(Constants.MediaTypes.MultipartFormData) &
                                                                                   ";\sboundary\=(?<Boundary>[\s\S]*)|(?<UrlEncoded>" &
                                                                                   Common.CreateRegExPattern(Constants.MediaTypes.UrlEncoded) &
                                                                                   "[^;\r\n]*)(;\s*" & charsetPattern & ")?)",
                                                                                   Text.RegularExpressions.RegexOptions.ExplicitCapture Or Text.RegularExpressions.RegexOptions.Singleline)
      Private _secondaryTypeIds As Dictionary(Of enumCollectionAction, List(Of String))
      Private _typeDefinition As String

      ''' <summary>
      ''' Sets value in list at position index
      ''' </summary>
      Private Sub SetAt(list As List(Of String), value As String, index As Integer)
         If list.Count <= index Then
            list.AddRange(CType(Array.CreateInstance(GetType(String), 1 + index - list.Count), String()))
         End If
         list(index) = value
      End Sub

      ''' <summary>
      ''' Returns ToString()-result from nested HttpContent
      ''' </summary>
      Public Overloads Function ToString(key As String) As String
         Dim content As HttpContent = Me.Content(key)
         Return If(content Is Nothing, Nothing, content.ToString())
      End Function

      Public ReadOnly UrlEncoding As System.Text.Encoding = System.Text.Encoding.UTF8

      ''' <summary>
      ''' Returns Value from nested HttpContent
      ''' </summary>
      Public Shadows ReadOnly Property Value(key As String) As Byte()
         Get
            Dim content As HttpContent = Me.Content(key)
            Return If(content Is Nothing, Nothing, content.Value)
         End Get
      End Property

      Protected Overrides Sub WriteHeaders(stream As System.IO.Stream)
         'a multipart-content has no headers to write
      End Sub

      Protected Overrides Sub WriteValue(stream As System.IO.Stream)
         If IsUrlEncoded Then
            'write as urlencoded
            If _contents.Count > 0 Then
               Dim buffer As Byte() = UrlEncoding.GetBytes(String.Join("&", (From content As HttpContent In _contents
                                                                             Where Not content.IsBinary
                                                                             Select sw.HttpUtility.UrlPathEncode(content.ContentDisposition) &
                                                                                    "=" & sw.HttpUtility.UrlEncode(content.ToString())).ToArray()))
               stream.Write(buffer, 0, buffer.Length)
            End If
         Else
            'write as multipart
            Dim boundaryStart As Byte() = System.Text.Encoding.UTF8.GetBytes("--" & Boundary & vbCrLf)
            Dim boundaryEnd As Byte() = System.Text.Encoding.UTF8.GetBytes("--" & Boundary & "--")
            For Each content As HttpContent In Contents
               stream.Write(boundaryStart, 0, boundaryStart.Length)
               content.WriteTo(stream)
            Next
            stream.Write(boundaryEnd, 0, boundaryEnd.Length)
         End If
      End Sub
   End Class
End Namespace