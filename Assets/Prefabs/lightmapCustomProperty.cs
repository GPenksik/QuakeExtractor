//using UnityEngine;
//using UnityEditor;
//using static bspMapReader.bspMapScriptable;

//[CustomPropertyDrawer(typeof(lightmap_t))]
//public class lightmapCustomProperty : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        base.OnGUI(position, property, label);  
//        EditorGUI.PrefixLabel(position, label);

//        Rect newPosition = position;
//        newPosition.y += 20f;
//        lightmap_t lightmap = (lightmap_t)property.boxedValue;
//        if (lightmap.LMData != null )
//        {
//            Debug.Log("STOP HERE");
//        }
//        //int LengthOfArray = 7;

//        //for (int i = 0; i < 8; i++)
//        //{
//        //    SerializedProperty row = rows.GetArrayElementAtIndex(i).FindPropertyRelative("row");
//        //    newPosition.LMHeight = 20;

//        //    if (row.arraySize != LengthOfArray)
//        //    {
//        //        row.arraySize = LengthOfArray;
//        //    }

//        //    newPosition.LMWidth = 20;

//        //    for (int j = 0; j < LengthOfArray; j++)
//        //    {
//        //        //Debug.Log("ED: j " + j);
//        //        EditorGUI.PropertyField(newPosition, row.GetArrayElementAtIndex(j), GUIContent.none);
//        //        newPosition.x += newPosition.LMWidth;
//        //    }

//        //    newPosition.x = position.x;
//        //    newPosition.y += 20;
//        //}
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return 20 * 9;
//    }
//}