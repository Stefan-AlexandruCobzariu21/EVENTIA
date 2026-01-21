using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace PIU
{
    internal class Action
    {
        private Shape _shape;
        private Point _point;
        private bool _creation;
        private bool _deletion;
        private bool _undoable;
        public Action(Shape shape, Point point, bool creation, bool deletion, bool undoable)
        {
            _shape = shape;
            _point = point;
            _creation = creation;
            _deletion = deletion;
            _undoable = undoable;
        }


        public Shape SHAPE
        {
            get { return _shape; }
            set { _shape = value; }
        }

        public Point POINT
        {
            get { return _point; }
            set { _point = value; }
        }

        public bool IsCreation
        {
            get { return _creation; }
            set { _creation = value; }
        }
        public bool IsDeletion
        {
            get { return _deletion; }
            set { _deletion = value; }
        }

        public bool IsUndoable
        {
            get { return _undoable; }
            set { _undoable = value; }
        }


    }
}
