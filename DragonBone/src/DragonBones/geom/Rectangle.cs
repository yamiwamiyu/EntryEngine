namespace DragonBones
 {
     /// <summary>
     /// - A Rectangle object is an area defined by its position, as indicated by its top-left corner point (x, y) and by its
     /// width and its height.<br/>
     /// The x, y, width, and height properties of the Rectangle class are independent of each other; changing the value of
     /// one property has no effect on the others. However, the right and bottom properties are integrally related to those
     /// four properties. For example, if you change the value of the right property, the value of the width property changes;
     /// if you change the bottom property, the value of the height property changes.
     /// </summary>
     /// <version>DragonBones 3.0</version>
     /// <language>en_US</language>

     /// <summary>
     /// - Rectangle 对象是按其位置（由它左上角的点 (x, y) 确定）以及宽度和高度定义的区域。<br/>
     /// Rectangle 类的 x、y、width 和 height 属性相互独立；更改一个属性的值不会影响其他属性。
     /// 但是，right 和 bottom 属性与这四个属性是整体相关的。例如，如果更改 right 属性的值，则 width
     /// 属性的值将发生变化；如果更改 bottom 属性，则 height 属性的值将发生变化。
     /// </summary>
     /// <version>DragonBones 3.0</version>
     /// <language>zh_CN</language>
     public class Rectangle
     {
         /// <summary>
         /// - The x coordinate of the top-left corner of the rectangle.
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>

         /// <summary>
         /// - 矩形左上角的 x 坐标。
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>zh_CN</language>
         public float x;
         /// <summary>
         /// - The y coordinate of the top-left corner of the rectangle.
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>

         /// <summary>
         /// - 矩形左上角的 y 坐标。
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>zh_CN</language>
         public float y;
         /// <summary>
         /// - The width of the rectangle, in pixels.
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>

         /// <summary>
         /// - 矩形的宽度（以像素为单位）。
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>zh_CN</language>
         public float width;
         /// <summary>
         /// - 矩形的高度（以像素为单位）。
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>

         /// <summary>
         /// - The height of the rectangle, in pixels.
         /// </summary>
         /// <default>0.0</default>
         /// <version>DragonBones 3.0</version>
         /// <language>zh_CN</language>
         public float height;

         /// <private/>
         public Rectangle()
         {
         }

         /// <private/>
         public void CopyFrom(Rectangle value)
         {
             this.x = value.x;
             this.y = value.y;
             this.width = value.width;
             this.height = value.height;
         }

         /// <private/>
         public void Clear()
         {
             this.x = this.y = 0.0f;
             this.width = this.height = 0.0f;
         }
     }
 }
