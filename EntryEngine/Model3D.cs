//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace EntryEngine
//{
//    public class CAMERA
//    {
//        private MATRIX view;
//        private bool changed;
//        public MATRIX View
//        {
//            get
//            {
//                if (changed)
//                {
//                    UpdateView();
//                }
//                return view;
//            }
//        }

//        public VECTOR3 Position;
//        public VECTOR3 Target;

//        public void SetPosition(VECTOR3 position)
//        {
//            Position = position;
//            changed = true;
//        }
//        public void SetPosition(float x, float y)
//        {
//            Position.X = x;
//            Position.Y = y;
//            changed = true;
//        }
//        public void SetPosition(float z)
//        {
//            Position.Z = z;
//            changed = true;
//        }
//        public void SetPosition(float x, float y, float z)
//        {
//            Position.X = x;
//            Position.Y = y;
//            Position.Z = z;
//            changed = true;
//        }
//        public void ChangePositionX(float x)
//        {
//            Position.X += x;
//            changed = true;
//        }
//        public void ChangePositionY(float y)
//        {
//            Position.Y += y;
//            changed = true;
//        }
//        public void ChangePositionZ(float z)
//        {
//            Position.Z += z;
//            changed = true;
//        }
//        public void ChangePosition(float x, float y, float z)
//        {
//            Position.X += x;
//            Position.Y += y;
//            Position.Z += z;
//            changed = true;
//        }
//        public void SetTarget(VECTOR3 target)
//        {
//            Target = target;
//            changed = true;
//        }
//        public void SetTarget(float x, float y)
//        {
//            Target.X = x;
//            Target.Y = y;
//            changed = true;
//        }
//        public void SetTarget(float z)
//        {
//            Target.Z = z;
//            changed = true;
//        }
//        public void SetTarget(float x, float y, float z)
//        {
//            Target.X = x;
//            Target.Y = y;
//            Target.Z = z;
//            changed = true;
//        }

//        public void UpdateView()
//        {
//            view = InternalUpdateView();
//            changed = false;
//        }
//        protected virtual MATRIX InternalUpdateView()
//        {
//            VECTOR3 forward = Target - Position;
//            VECTOR3 side = VECTOR3.Cross(forward, VECTOR3.Up);
//            VECTOR3 up = VECTOR3.Cross(forward, side);
//            return MATRIX.CreateLookAt(Position, Target, up);
//        }
//    }
//    public class CAMERAFree : CAMERA
//    {
//        private VECTOR3 position;
//        public float Yaw;
//        public float Pitch;

//        protected override MATRIX InternalUpdateView()
//        {
//            VECTOR3 translation = Position - position;
//            this.position = Position;
//            // Calculate the rotation matrix
//            MATRIX rotation = MATRIX.CreateFromYawPitchRoll(Yaw, Pitch, 0);
//            // Offset the position and reset the translation
//            translation = VECTOR3.Transform(translation, rotation);
//            Position += translation;
//            // Calculate the new target
//            VECTOR3 forward = VECTOR3.Transform(VECTOR3.Forward, rotation);
//            Target = Position + forward;
//            // Calculate the up vector
//            VECTOR3 up = VECTOR3.Transform(VECTOR3.Up, rotation);
//            // Calculate the view matrix
//            return MATRIX.CreateLookAt(Position, Target, up);
//        }
//    }
//    public struct MODEL3D
//    {
//        public VECTOR3 Position;
//        public VECTOR3 Rotation;
//        public VECTOR3 Scale;
//        private MATRIX matrix;
//        private bool changed;
//        public MATRIX Matrix
//        {
//            get
//            {
//                if (changed)
//                {
//                    UpdateMatrix();
//                }
//                return matrix;
//            }
//        }
//        public void UpdateMatrix()
//        {
//            matrix = MATRIX.CreateScale(Scale)
//                * MATRIX.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)
//                * MATRIX.CreateTranslation(Position);
//            changed = false;
//        }
//        public void SetPosition(VECTOR3 position)
//        {
//            Position = position;
//            changed = true;
//        }
//        public void SetPosition(float x, float y)
//        {
//            Position.X = x;
//            Position.Y = y;
//            changed = true;
//        }
//        public void SetPosition(float z)
//        {
//            Position.Z = z;
//            changed = true;
//        }
//        public void SetPosition(float x, float y, float z)
//        {
//            Position.X = x;
//            Position.Y = y;
//            Position.Z = z;
//            changed = true;
//        }
//        public void ChangePositionX(float x)
//        {
//            Position.X += x;
//            changed = true;
//        }
//        public void ChangePositionY(float y)
//        {
//            Position.Y += y;
//            changed = true;
//        }
//        public void ChangePositionZ(float z)
//        {
//            Position.Z += z;
//            changed = true;
//        }
//        public void ChangePosition(float x, float y, float z)
//        {
//            Position.X += x;
//            Position.Y += y;
//            Position.Z += z;
//            changed = true;
//        }
//        public void SetScale(float x, float y, float z)
//        {
//            Scale.X = x;
//            Scale.Y = y;
//            Scale.Z = z;
//            changed = true;
//        }
//        public void SetScale(VECTOR3 scale)
//        {
//            Scale = scale;
//            changed = true;
//        }
//        public void SetScale(float scale)
//        {
//            Scale.X = scale;
//            Scale.Y = scale;
//            Scale.Z = scale;
//            changed = true;
//        }
//        public void SetScale(float x, float y)
//        {
//            Scale.X = x;
//            Scale.Y = y;
//            changed = true;
//        }
//        public void ChangeScale(float scale)
//        {
//            Scale.X += scale;
//            Scale.Y += scale;
//            Scale.Z += scale;
//            changed = true;
//        }
//        public void ChangeScale(float x, float y, float z)
//        {
//            Scale.X += x;
//            Scale.Y += y;
//            Scale.Z += z;
//            changed = true;
//        }
//        public void SetRotation(float x, float y, float z)
//        {
//            Rotation.X = x;
//            Rotation.Y = y;
//            Rotation.Z = z;
//            changed = true;
//        }
//        public void SetRotationX(float x)
//        {
//            Rotation.X = x;
//            changed = true;
//        }
//        public void SetRotationY(float y)
//        {
//            Rotation.Y = y;
//            changed = true;
//        }
//        public void SetRotationZ(float z)
//        {
//            Rotation.Z = z;
//            changed = true;
//        }
//        public void ChangeRotation(float x, float y, float z)
//        {
//            Rotation.X += x;
//            Rotation.Y += y;
//            Rotation.Z += z;
//            changed = true;
//        }
//        public void ChangeRotationX(float x)
//        {
//            Rotation.X += x;
//            changed = true;
//        }
//        public void ChangeRotationY(float y)
//        {
//            Rotation.Y += y;
//            changed = true;
//        }
//        public void ChangeRotationZ(float z)
//        {
//            Rotation.Z += z;
//            changed = true;
//        }
//        public void ChangeRotationAngle(float x, float y, float z)
//        {
//            Rotation.X = _MATH.ToRadian(_MATH.ToDegree(Rotation.X) + x);
//            Rotation.Y = _MATH.ToRadian(_MATH.ToDegree(Rotation.X) + y);
//            Rotation.Z = _MATH.ToRadian(_MATH.ToDegree(Rotation.X) + z);
//            changed = true;
//        }
//        public void ChangeRotationAngleX(float x)
//        {
//            Rotation.X = _MATH.ToRadian(_MATH.ToDegree(Rotation.X) + x);
//            changed = true;
//        }
//        public void ChangeRotationAngleY(float y)
//        {
//            Rotation.Y = _MATH.ToRadian(_MATH.ToDegree(Rotation.X) + y);
//            changed = true;
//        }
//        public void ChangeRotationAngleZ(float z)
//        {
//            Rotation.Z = _MATH.ToRadian(_MATH.ToDegree(Rotation.X) + z);
//            changed = true;
//        }
//    }
//    public class Model3DSimple : Content
//    {
        
//        /*
//         * 1. 空间距离和现实距离的映射（例如 1 = 1米）
//         * 2. 绘制线框模式用两点一线的绘制方式更加方便快速
//         * 3. 也可以考虑点云(point cloud)(3D扫描仪)的形式仅显示点，不过点云最终还是会被生成多边形网格或三角形网格
//         */
//        public override bool IsDisposed
//        {
//            get { throw new NotImplementedException(); }
//        }
//        protected internal override void InternalDispose()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
