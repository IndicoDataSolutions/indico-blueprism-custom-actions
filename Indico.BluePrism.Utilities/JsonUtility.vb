Imports System.Data
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

''' <summary>
''' Source: https://github.com/blue-prism/json-utility/blob/master/BPA%20Object%20-%20Utility%20-%20JSON.xml#L84-L300
''' https://github.com/blue-prism/json-utility is available under MIT License
''' </summary>
Public Class JsonUtility
    Private Class JSON
        Public Const Array As String = "JSON:Array"
        Public Const Null As String = "JSON:Null"
    End Class

    Private mUseNewParseMethod As Boolean
    Public Function ConvertToJSON(ByVal dt As DataTable) As String
        Dim o As Object = SerialiseGeneric(dt, True)
        Return JsonConvert.SerializeObject(o)
    End Function
    Public Function SerialiseGeneric(ByVal o As Object, ByVal removeArray As Boolean) As Object
        Dim dt As DataTable = TryCast(o, DataTable)
        If dt IsNot Nothing Then
            Return SerialiseDataTable(dt)
        End If
        Dim dr As DataRow = TryCast(o, DataRow)
        If dr IsNot Nothing Then
            Return SerialiseDataRow(dr, removeArray)
        End If
        Dim s As String = TryCast(o, String)
        If s IsNot Nothing AndAlso s = JSON.Null Then
            Return Nothing
        End If
        If o IsNot Nothing Then
            Return o
        End If
        Return Nothing
    End Function
    Public Function SerialiseDataTable(ByVal dt As DataTable) As Object
        If IsSingleRow(dt) Then
            Return SerialiseGeneric(dt.Rows(0), False)
        Else
            Dim ja As New JArray()
            For Each r As DataRow In dt.Rows
                ja.Add(SerialiseGeneric(r, True))
            Next
            Return ja
        End If
    End Function
    Public Function IsSingleRow(ByVal dt As DataTable) As Boolean
        If dt.ExtendedProperties.Contains("SingleRow") Then
            Return CBool(dt.ExtendedProperties("SingleRow"))
        End If
        'Fallback for older versions of blueprism
        Return dt.Rows.Count = 1
    End Function
    Public Function SerialiseDataRow(ByVal dr As DataRow, ByVal removeArray As Boolean) As Object
        Dim jo As New JObject()
        For Each c As DataColumn In dr.Table.Columns
            Dim s As String = c.ColumnName
            If removeArray AndAlso s = JSON.Array Then
                Return SerialiseGeneric(dr(s), True)
            End If
            jo(s) = JToken.FromObject(SerialiseGeneric(dr(s), False))
        Next
        Return jo
    End Function
    Public Function ConvertToDataTable(ByVal json As String) As DataTable
        Dim o As Object = JsonConvert.DeserializeObject(json)
        Return DirectCast(DeserialiseGeneric(o, True), DataTable)
    End Function
    Private Function DeserialiseGeneric(ByVal o As Object, ByVal populate As Boolean) As Object
        Dim a As JArray = TryCast(o, JArray)
        If a IsNot Nothing Then
            Return If(mUseNewParseMethod,
            DeserialiseArrayWithoutJArray(a, populate),
            DeserialiseArray(a, populate)
        )
        End If
        Dim jo As JObject = TryCast(o, JObject)
        If jo IsNot Nothing Then
            Return DeserialiseObject(jo, populate)
        End If
        Dim jv As JValue = TryCast(o, JValue)
        If jv IsNot Nothing Then
            Return jv.Value
        End If
        Return JSON.Null
    End Function
    Private Function GetKey(ByVal kv As KeyValuePair(Of String, JToken)) As String
        If kv.Key IsNot Nothing Then
            Return kv.Key.ToString()
        End If
        Return ""
    End Function
    Private Function DeserialiseObject(ByVal o As JObject, ByVal populate As Boolean) As DataTable
        Dim dt As New DataTable
        For Each kv As KeyValuePair(Of String, JToken) In o
            Dim type As Type = GetTypeOf(DeserialiseGeneric(kv.Value, False))
            dt.Columns.Add(GetKey(kv), type)
        Next
        If populate Then
            Dim dr As DataRow = dt.NewRow()
            For Each kv As KeyValuePair(Of String, JToken) In o
                dr(GetKey(kv)) = DeserialiseGeneric(kv.Value, True)
            Next
            dt.Rows.Add(dr)
        End If
        Return dt
    End Function
    Private Function DeserialiseArrayWithoutJArray(ByVal jarr As JArray, ByVal populate As Boolean) As DataTable
        Dim dt As New DataTable
        Dim first As Type = Nothing
        For Each e As Object In jarr
            If first Is Nothing Then
                first = GetTypeOf(DeserialiseGeneric(e, False))
            End If
            If GetTypeOf(DeserialiseGeneric(e, False)) IsNot first Then
                Throw New Exception("Data Type mismatch in array")
            End If
        Next
        Dim columns As New Specialized.OrderedDictionary()
        Dim allTypesInColumnsMatch = jarr.All(
        Function(e)
            If Not TypeOf e Is JObject Then Return False
            For Each pair As KeyValuePair(Of String, JToken) In DirectCast(e, JObject)
                Dim val As Object = pair.Value
                If TypeOf val Is JValue Then val = CType(val, JValue).Value
                Dim tp = If(val, CObj("")).GetType()
                If columns.Contains(pair.Key) Then
                    If tp <> columns(pair.Key) Then Return False
                Else
                    columns(pair.Key) = tp
                End If
            Next
            Return True
        End Function
    )
        If allTypesInColumnsMatch Then
            For Each pair As DictionaryEntry In columns
                Dim key As String = CStr(pair.Key)
                Dim tp As Type = CType(pair.Value, Type)
                If tp = GetType(JObject) OrElse tp = GetType(JArray) Then
                    dt.Columns.Add(key, GetType(DataTable))
                Else
                    dt.Columns.Add(key, tp)
                End If
            Next
        ElseIf first IsNot Nothing Then
            dt.Columns.Add(JSON.Array, first)
        End If
        If populate Then
            For Each e As Object In jarr
                Dim dr As DataRow = dt.NewRow()
                If allTypesInColumnsMatch Then
                    For Each pair As KeyValuePair(Of String, JToken) In DirectCast(e, JObject)
                        dr(pair.Key) = DeserialiseGeneric(pair.Value, True)
                    Next
                Else
                    dr(JSON.Array) = DeserialiseGeneric(e, True)
                End If
                dt.Rows.Add(dr)
            Next
        End If
        Return dt
    End Function
    Private Function DeserialiseArray(ByVal o As JArray, ByVal populate As Boolean) As DataTable
        Dim dt As New DataTable
        Dim first As Type = Nothing
        For Each e As Object In o
            If first Is Nothing Then
                first = GetTypeOf(DeserialiseGeneric(e, False))
            End If
            If GetTypeOf(DeserialiseGeneric(e, False)) IsNot first Then
                Throw New Exception("Data Type mismatch in array")
            End If
        Next
        If first IsNot Nothing Then
            dt.Columns.Add(JSON.Array, first)
        End If
        If populate Then
            For Each e As Object In o
                Dim dr As DataRow = dt.NewRow()
                dr(JSON.Array) = DeserialiseGeneric(e, True)
                dt.Rows.Add(dr)
            Next
        End If
        Return dt
    End Function
    Private Function GetTypeOf(ByVal o As Object) As Type
        If o Is Nothing Then Return GetType(String)
        Return o.GetType
    End Function

End Class