namespace EntryEngine.DragonBone.DBCore
 {
     /// <summary>
     /// - The Point object represents a location in a two-dimensional coordinate system.
     /// </summary>
     /// <version>DragonBones 3.0</version>
     /// <language>en_US</language>

     /// <summary>
     /// - Point 对象表示二维坐标系统中的某个位置。
     /// </summary>
     /// <version>DragonBones 3.0</version>
     /// <language>zh_CN</language>
     public class Point
     {
         /// <summary>
         /// - The horizontal coordinate.
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>

         /// <summary>
         /// - 该点的水平坐标。
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>zh_CN</language>
         public float x = 0.0f;
         /// <summary>
         /// - The vertical coordinate.
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>

         /// <summary>
         /// - 该点的垂直坐标。
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>zh_CN</language>
         public float y = 0.0f;

         /// <summary>
         /// - Creates a new point. If you pass no parameters to this method, a point is created at (0,0).
         /// </summary>
         /// <param name="x">- The horizontal coordinate.</param>
         /// <param name="y">- The vertical coordinate.</param>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>

         /// <summary>
         /// - 创建一个 egret.Point 对象.若不传入任何参数，将会创建一个位于（0，0）位置的点。
         /// </summary>
         /// <param name="x">- 该对象的x属性值，默认为 0.0。</param>
         /// <param name="y">- 该对象的y属性值，默认为 0.0。</param>
         /// <version>DragonBones 3.0</version>
         /// <language>zh_CN</language>
         public Point()
         {
         }

         /// <private/>
         public void CopyFrom(Point value)
         {
             this.x = value.x;
             this.y = value.y;
         }

         /// <private/>
         public void Clear()
         {
             this.x = this.y = 0.0f;
         }

         public override string ToString()
         {
             return string.Format("x: {0} y: {1}", x, y);
         }
     }
 }
