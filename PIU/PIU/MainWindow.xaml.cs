using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using System.Text.Json;
using System.Windows.Automation.Provider;
using Color = System.Windows.Media.Color;
using System;
using System.Windows.Controls.Ribbon.Primitives;
using System.Diagnostics.Eventing.Reader;

namespace PIU
{
    public partial class MainWindow : Window
    {
        private Shape _currentDraggingShape; 
        private Shape _focusedShape;
        private bool _isDragging; 
        private Stack<Action> _redoActions;
        private Stack<Action> _historyActions;
        private List<Shape> _mese;
       // private bool Intersect=false;

        private Line _horizontalGuideLine;
        private Line _verticalGuideLine;
        private Line _leftGuideLine;
        private Line _rightGuideLine;
        private Line _topGuideLine;
        private Line _bottomGuideLine;

        List<Shape> intersectedVertical ;
        List<Shape> intersectedHorizontal ;
        List<Shape> intersectedRight ;
        List<Shape> intersectedLeft ;
        List<Shape> intersectedTop ;
        List<Shape> intersectedBottom;
        List<Line> guideLines;
      
        private bool _isAligned = false;
       

       
        public MainWindow()
        {
            InitializeComponent();
            _redoActions = new Stack<Action>();
            _historyActions = new Stack<Action>();
           
            _mese = new List<Shape>();
            
        }
        

        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            if (_focusedShape != null)
            {
                _focusedShape = null;
            }

            if (sender is Shape originalShape)
            {
               
                Shape copy = CreateShapeCopy(originalShape);
                _redoActions.Clear();

                System.Windows.Point initialPosition = e.GetPosition(canvas);
                if (copy.Name == "MusicPlatform") {
                   
                    initialPosition.X = canvas.ActualWidth / 2 -copy.Width/2; 
                    initialPosition.Y = 0;

                }
                Canvas.SetLeft(copy, initialPosition.X);
                Canvas.SetTop(copy, initialPosition.Y);

                AddGuideLines(copy);                
                AttachEvents(copy);

                Action action = new Action(copy, new Point(0, 0), true, false, true);
                _historyActions.Push(action);
                //MessageBox.Show("Actiune : " + action.SHAPE.ToString() + " " + action.POINT.ToString() + " ");

               
                _currentDraggingShape = copy;
                _isDragging = true;
                _isAligned = false;

                _currentDraggingShape.CaptureMouse();
                _currentDraggingShape.Focus();
            }
        }

        private void A_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                try
                {
                    AlignObject();
                }
                catch (ExceptionInsufficientSpace exc)
                {
                    MessageBox.Show(exc.Message);
                }

                catch
                {
                    MessageBox.Show("Forma trebuie sa intersecteze un obiect pentru a putea fi aliniat");
                }

            }
        }
        private void AlignObject()
        {
           
            DetectIntersectedShape();
            
            if (!(intersectedVertical.Count == 0) && !(intersectedRight.Count == 0) && !(intersectedLeft.Count == 0))
            { 
                intersectedRight.Clear(); intersectedLeft.Clear();
            }
            if ((!(intersectedVertical.Count == 0) && !(intersectedRight.Count == 0)) || (!(intersectedLeft.Count == 0)&& !(intersectedVertical.Count == 0)))
            {
                intersectedVertical.Clear(); 
            }

            Shape closestShape = null;
            double minDistance = double.MaxValue;
            Line gline = null;

 
            void FindClosestFromList(List<Shape> intersectedShapes)
            {
                foreach (var shape in intersectedShapes)
                {
                    double distance = calculDistance(shape);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestShape = shape;
                    }
                    if (intersectedShapes == intersectedVertical) gline= _verticalGuideLine;
                    if (intersectedShapes == intersectedHorizontal) gline= _horizontalGuideLine;
                    if (intersectedShapes == intersectedRight) gline= _rightGuideLine;
                    if (intersectedShapes == intersectedLeft) gline= _leftGuideLine;
                    if (intersectedShapes == intersectedTop) gline= _topGuideLine;
                    if (intersectedShapes ==intersectedBottom) gline= _bottomGuideLine;
                }
            }
            FindClosestFromList(intersectedVertical);
            FindClosestFromList(intersectedHorizontal);
            FindClosestFromList(intersectedRight);
            FindClosestFromList(intersectedLeft);
            FindClosestFromList(intersectedTop);
            FindClosestFromList(intersectedBottom);

     
            if (_currentDraggingShape.Name == Chair.Name)
            {
                if (closestShape.Name == masa.Name && closestShape != null && masa != null)
                {
                    
                        AlignChairToIntersected(closestShape, gline);
                 }

            }

            if (_currentDraggingShape.Name == masa.Name)
            {
                if (closestShape.Name == MusicPlatform.Name && closestShape != null && MusicPlatform != null)
                {
                        double distance1 = 100;
                        double distance2 = Chair.Height+30;
                        AlignTableToIntersected(closestShape, gline,distance1,distance2);
                 }

            }
            if (_currentDraggingShape.Name == masa.Name)
            {
                if (closestShape.Name == masa.Name && closestShape != null && masa != null)
                {
                    double distance1 =100;
                    double distance2 = 2*Chair.Height + 50;
                    AlignTableToIntersected(closestShape, gline,distance1,distance2);
                }

            }

            if (_isAligned)
            {
                _isDragging = false;
                _currentDraggingShape.ReleaseMouseCapture();
                HideGuideLines();
            }
            
        }

        private void DetectIntersectedShape() {
            intersectedVertical = DetectIntersection(_verticalGuideLine);
            // MessageBox.Show("intersectii detectate: " + intersectedVertical.Count );
            intersectedHorizontal = DetectIntersection(_horizontalGuideLine);
            intersectedRight = DetectIntersection(_rightGuideLine);
            intersectedLeft = DetectIntersection(_leftGuideLine);
            intersectedTop = DetectIntersection(_topGuideLine);
            intersectedBottom = DetectIntersection(_bottomGuideLine);

        }
        
        private void AlignChairToIntersected(Shape intersected,Line gline)
        {
                
            if (gline == _bottomGuideLine||gline== _topGuideLine||gline==_horizontalGuideLine)
            {
                double leftAfterAllign;
                if (Canvas.GetLeft(_currentDraggingShape)>Canvas.GetLeft(intersected))
                {
                    leftAfterAllign =Canvas.GetLeft(intersected) + intersected.Width +_currentDraggingShape.ActualWidth;
                }
                else
                {
                    leftAfterAllign =Canvas.GetLeft(intersected) - 2 *_currentDraggingShape.ActualWidth;
                }
                Canvas.SetLeft(_currentDraggingShape,leftAfterAllign);

                if (gline == _horizontalGuideLine || gline == _bottomGuideLine || gline == _topGuideLine)
                {
                    Canvas.SetTop(_currentDraggingShape, Canvas.GetTop(intersected) + intersected.ActualHeight/ 2 - _currentDraggingShape.ActualHeight / 2);

                }
            }
            else
            {
                double topAfterAllign;
                if (Canvas.GetTop(_currentDraggingShape) > Canvas.GetTop(intersected))
                {
                    topAfterAllign = Canvas.GetTop(intersected) + intersected.Height + _currentDraggingShape.ActualHeight;
                }
                else
                {
                    topAfterAllign = Canvas.GetTop(intersected) - 2 * _currentDraggingShape.ActualHeight;
                }
                Canvas.SetTop(_currentDraggingShape, topAfterAllign);
                if (gline==_verticalGuideLine)
                {
                    Canvas.SetLeft(_currentDraggingShape, Canvas.GetLeft(intersected)+intersected.ActualWidth/ 2 - _currentDraggingShape.ActualWidth/ 2);
                }
                if (gline== _leftGuideLine)
                {
                    Canvas.SetLeft(_currentDraggingShape, Canvas.GetLeft(intersected)+intersected.ActualWidth/ 2 + _currentDraggingShape.ActualWidth);
                }
                if (gline== _rightGuideLine)
                {
                    Canvas.SetLeft(_currentDraggingShape, Canvas.GetLeft(intersected));
                }
            }

            setAligned();
            
        }
        private void AlignTableToIntersected(Shape intersected, Line gline,double distance1, double distance2)
        {
            
            if (gline == _bottomGuideLine || gline == _topGuideLine || gline == _horizontalGuideLine)
            {
                double leftAfterAllign;
                //cand am obiectul in partea dreapta a obiectului intersctat
                if (Canvas.GetLeft(_currentDraggingShape) > Canvas.GetLeft(intersected))
                {
         
                    if ((Canvas.GetLeft(intersected) + intersected.Width + distance1 + _currentDraggingShape.Width) <= canvas.ActualWidth)
                    {
                    //    MessageBox.Show("Valori:" + h);
                        leftAfterAllign = Canvas.GetLeft(intersected) + intersected.Width + distance2; 
                    }
                    else
                    {
                        throw new ExceptionInsufficientSpace("Nu exista suficient spatiu pentru aliniere");
                    }
                }
                else
                {
                    if ((Canvas.GetLeft(intersected) - _currentDraggingShape.Width - distance1) >= 0)
                    {
                        leftAfterAllign = Canvas.GetLeft(intersected) - _currentDraggingShape.ActualWidth - distance2;
                    }
                    else
                    {
                        throw new ExceptionInsufficientSpace("Nu exista suficient spatiu pentru aliniere");
                    }
               }

                Canvas.SetLeft(_currentDraggingShape, leftAfterAllign);
                if (gline == _horizontalGuideLine || gline == _bottomGuideLine || gline == _topGuideLine)
                {
                    Canvas.SetTop(_currentDraggingShape, Canvas.GetTop(intersected) + intersected.ActualHeight / 2 - _currentDraggingShape.ActualHeight / 2);
                }
            }
            // cand fac cu liniile de pe verticlaa

            else if (gline == _verticalGuideLine || gline == _leftGuideLine || gline == _rightGuideLine)
            {
                double topAfterAllign;
                // dedesubt
                if (Canvas.GetTop(_currentDraggingShape) > Canvas.GetTop(intersected))
                {

                    if (Canvas.GetTop(intersected) + intersected.Height + distance1 + _currentDraggingShape.ActualHeight <= canvas.ActualHeight)
                    {
                        topAfterAllign = Canvas.GetTop(intersected) + intersected.Height + distance2;
                    }
                    else
                    {
                        throw new ExceptionInsufficientSpace("Nu este suficient spatiu pentru aliniere ");
                    }

                   
                }
                // cand ob  curent este deasupra obiect intersctat
                else
                {
                    if ((Canvas.GetTop(intersected) - _currentDraggingShape.ActualHeight - distance1) >= 0)
                    {
                        topAfterAllign = Canvas.GetTop(intersected) - _currentDraggingShape.ActualHeight - distance2;
                    }
                    else
                    {
                        throw new ExceptionInsufficientSpace("Nu este suficient spatiu pentru aliniere ");
                    }
                }
                Canvas.SetTop(_currentDraggingShape, topAfterAllign);
                
                Canvas.SetLeft(_currentDraggingShape, Canvas.GetLeft(intersected) + intersected.ActualWidth / 2 - _currentDraggingShape.ActualWidth / 2);
                
               
            }

            setAligned();

        }



        void setAligned() {
            Point point;
            Action action;
            _isAligned = true;
            point = new Point(Canvas.GetLeft(_currentDraggingShape), Canvas.GetTop(_currentDraggingShape));
            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;
            action = new Action(_currentDraggingShape, point, false, false, true);
            _historyActions.Push(action);

        }

     
        double calculDistance(Shape intersected)
        {
           if (intersected == null) { return double.MaxValue; }

            double current_coltLeft = Canvas.GetLeft(_currentDraggingShape);
            double current_coltTop = Canvas.GetTop(_currentDraggingShape);
            double current_coltRight = current_coltLeft + _currentDraggingShape.Width;
            double current_coltBottom = current_coltTop + _currentDraggingShape.Height;

            double intersectedLeft = Canvas.GetLeft(intersected);
            double intersectedTop = Canvas.GetTop(intersected);
            double intersectedRight = intersectedLeft + intersected.Width;
            double intersectedBottom = intersectedTop + intersected.Height;

            double dist = double.MaxValue;
        if (intersectedRight>current_coltLeft && intersectedLeft <current_coltRight)
            {
                dist = Math.Min(dist, Math.Abs(current_coltTop-intersectedBottom));
                dist = Math.Min(dist, Math.Abs(current_coltBottom -intersectedTop));
            }
            if (intersectedBottom >current_coltTop &&intersectedTop<current_coltBottom)
            {
                dist = Math.Min(dist,Math.Abs(current_coltLeft -intersectedRight));
                dist = Math.Min(dist,Math.Abs(current_coltRight - intersectedLeft));
            }
            return dist;
        }


        private void AddGuideLines(Shape shape)
        {
          _horizontalGuideLine=new Line();
            StyleForGuideLine(_horizontalGuideLine);
            
            _verticalGuideLine= new Line();
            StyleForGuideLine(_verticalGuideLine);
            
            _leftGuideLine =new Line();
            StyleForGuideLine(_leftGuideLine);
            
            _rightGuideLine =new Line();
            StyleForGuideLine(_rightGuideLine);
            
            _topGuideLine =new Line();
            StyleForGuideLine(_topGuideLine);

            _bottomGuideLine =new Line();
            StyleForGuideLine(_bottomGuideLine);

            canvas.Children.Add(_horizontalGuideLine);
            canvas.Children.Add(_verticalGuideLine);
            canvas.Children.Add(_leftGuideLine);
            canvas.Children.Add(_rightGuideLine);
            canvas.Children.Add(_topGuideLine);
            canvas.Children.Add(_bottomGuideLine);
        }
        private void StyleForGuideLine(Line line)
        {
            line.Stroke = System.Windows.Media.Brushes.DeepSkyBlue;
            line.StrokeThickness = 1.5;
            line.Opacity = 0.6;
            line.StrokeDashArray = new DoubleCollection { 4, 2 };
        }

        private void NewShape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Shape shape)
            {
                _currentDraggingShape = shape;
                _focusedShape = shape;
                _isDragging = true;
                _isAligned = false;
                 _currentDraggingShape.CaptureMouse();
            }
        }


      private void NewShape_MouseMove(object sender, MouseEventArgs e)
{
    if (_isDragging && _currentDraggingShape != null && !_isAligned)
    {
        Point currentPosition = e.GetPosition(canvas);
        double canvasLeft = 0;
        double canvasTop = 0;
        double canvasRight = canvas.ActualWidth - _currentDraggingShape.Width;
        double canvasBottom = canvas.ActualHeight - _currentDraggingShape.Height;

        double proposedLeft = Math.Max(canvasLeft, Math.Min(currentPosition.X - _currentDraggingShape.Width / 2, canvasRight));
        double proposedTop = Math.Max(canvasTop, Math.Min(currentPosition.Y - _currentDraggingShape.Height / 2, canvasBottom));

            if (_currentDraggingShape.Name == MusicPlatform.Name)
            {
                RestrictAreaForMusicPlatform(ref proposedLeft, ref proposedTop, canvasLeft, canvasRight, canvasTop, canvasBottom);
            }
                    
                

            if (!IsColliding(_currentDraggingShape, proposedLeft, proposedTop))
            {
                Canvas.SetLeft(_currentDraggingShape, proposedLeft);
                Canvas.SetTop(_currentDraggingShape, proposedTop);
                UpdateCoordGuideLines(proposedLeft, proposedTop);

                List<Line> guideLines = new List<Line>
                {
                    _leftGuideLine,
                    _rightGuideLine,
                    _topGuideLine,
                    _bottomGuideLine,
                    _verticalGuideLine
                };

                foreach (Line guideLine in guideLines)
                {
                    ChangeLineOnIntersection(guideLine);
                }
            }
        }
    }



        private void RestrictAreaForMusicPlatform(ref double proposedLeft, ref double proposedTop, double canvasLeft, double canvasRight, double canvasTop, double canvasBottom)
        {
            double canvasCenterX = canvas.ActualWidth / 2;
            double leftLimit = canvasCenterX -(_currentDraggingShape.Width / 2);
            double rightLimit = canvasCenterX +(_currentDraggingShape.Width / 2);

           
            if (proposedLeft >= leftLimit-5 && proposedLeft <=rightLimit+5)
            {
                proposedLeft = leftLimit+5; 
            }
            else {
                double currentLeft = Canvas.GetLeft(_currentDraggingShape);
                double currentTop = Canvas.GetTop(_currentDraggingShape);
                proposedLeft = currentLeft;
                proposedTop = currentTop; }
        }





        private void ChangeLineOnIntersection(Line gline)
        {
            List<Shape> i_shape = DetectIntersection(gline);
            bool intersection = false;

            foreach (Shape shape in i_shape)
            {
                if (shape != null)
                {
                    gline.Stroke = System.Windows.Media.Brushes.DeepPink;
                    gline.Visibility = Visibility.Visible;
                    intersection = true;
                   }
            }
            if (intersection==false)
            {
                gline.Stroke = System.Windows.Media.Brushes.DeepSkyBlue;
                if (gline != _horizontalGuideLine && gline != _verticalGuideLine)
                {
                    gline.Visibility = Visibility.Hidden;
                }
            }
        }
        private void setLineCoord(Line line,double Xc1,double Yc1,double Xc2,double Yc2) {
            line.X1 = Xc1;
            line.Y1 = Yc1;
            line.X2 = Xc2;
            line.Y2 = Yc2;
        }

        private List<Shape> DetectIntersection(Line guideLine) { 
             List<Shape> intersectedShapes = new List<Shape>();

        foreach (Shape shape in canvas.Children)
       {
         if ( shape != _currentDraggingShape && !(shape is Line))
         {
            double shapeLeft = Canvas.GetLeft(shape);
            double shapeTop = Canvas.GetTop(shape);
            double shapeRight = shapeLeft + shape.Width;
            double shapeBottom = shapeTop + shape.Height;
            double gLineXCoord = guideLine.X1;
            double gLineYCoord = guideLine.Y1;
               if (guideLine == _verticalGuideLine || guideLine == _leftGuideLine|| guideLine == _rightGuideLine)
                {
                   if (gLineXCoord >= shapeLeft && gLineXCoord <= shapeRight)
                     {
                            if((_currentDraggingShape.Name == masa.Name && shape.Name == masa.Name) || (_currentDraggingShape.Name == Chair.Name && shape.Name == masa.Name) || (_currentDraggingShape.Name == masa.Name && shape.Name == MusicPlatform.Name))
                            {
                                intersectedShapes.Add(shape);
                            }
                           
                                
                      }
               }
               else if (guideLine == _horizontalGuideLine || guideLine == _bottomGuideLine ||guideLine ==_topGuideLine)
                {
                        
                        if (gLineYCoord >= shapeTop&&gLineYCoord <= shapeBottom)
                        {
                            if ((_currentDraggingShape.Name == masa.Name && shape.Name == masa.Name) || (_currentDraggingShape.Name == Chair.Name && shape.Name == masa.Name) || (_currentDraggingShape.Name==masa.Name && shape.Name == MusicPlatform.Name))

                            {
                                intersectedShapes.Add(shape);
                            }
                           
 
                        }
                    }
                   
        }
    }
        return intersectedShapes; 
 }


        private void UpdateCoordGuideLines(double left, double top)
        {
            double right = left + _currentDraggingShape.Width;
            double bottom = top + _currentDraggingShape.Height;

            double vertical_centerX = left + _currentDraggingShape.Width / 2;
            setLineCoord(_verticalGuideLine,vertical_centerX,0,vertical_centerX,canvas.ActualHeight);
            _verticalGuideLine.Visibility = Visibility.Visible;

            double orizontal_centerY = top + _currentDraggingShape.Height / 2;
            setLineCoord(_horizontalGuideLine,0,orizontal_centerY,canvas.ActualWidth,orizontal_centerY);
            _horizontalGuideLine.Visibility = Visibility.Visible;

            setLineCoord(_leftGuideLine, left, 0, left, canvas.ActualHeight);
        //    _leftGuideLine.Visibility = Visibility.Visible;

            setLineCoord(_rightGuideLine, right, 0, right, canvas.ActualHeight);
       //     _rightGuideLine.Visibility = Visibility.Visible;

            setLineCoord(_topGuideLine, 0, top, canvas.ActualWidth,top);
   //         _topGuideLine.Visibility = Visibility.Visible;

            setLineCoord(_bottomGuideLine,0, bottom,canvas.ActualWidth, bottom);
    //        _bottomGuideLine.Visibility = Visibility.Visible;
        }
        private void HideGuideLines()
        {
            _horizontalGuideLine.Visibility = Visibility.Hidden;
            _verticalGuideLine.Visibility = Visibility.Hidden;
            _leftGuideLine.Visibility = Visibility.Hidden;
            _rightGuideLine.Visibility = Visibility.Hidden;
            _topGuideLine.Visibility = Visibility.Hidden;
            _bottomGuideLine.Visibility = Visibility.Hidden;
        }

        private void NewShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && _currentDraggingShape != null)
            {
                
                _currentDraggingShape.ReleaseMouseCapture();
                HideGuideLines();

                Point point = new Point(Canvas.GetLeft(_currentDraggingShape), Canvas.GetTop(_currentDraggingShape));
                if(point.X < 0)
                    point.X = 0;
                if(point.Y < 0)
                    point.Y = 0;

                var pointHistory = GetCoordinatesInHistory(_currentDraggingShape);
                if (pointHistory.X < 0 || Math.Abs(point.X - pointHistory.X) > 5 || Math.Abs(point.Y - pointHistory.Y) > 5)
                {
                    Action action = new Action(_currentDraggingShape, point, false, false, true);
                    _historyActions.Push(action);
                    //MessageBox.Show("Actiune : " + action.SHAPE.ToString() + " " + action.POINT.ToString() + " ");
                }
                

                _currentDraggingShape = null;
                _isDragging = false;

            }
        }
        


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (OptionsComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                string savesFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Saves");
                if (!Directory.Exists(savesFolder))
                {
                    Directory.CreateDirectory(savesFolder);
                }

                if (selectedValue == "Image")
                {
                    SaveCanvasAsImage(savesFolder);
                }
                else if (selectedValue == "Layout")
                {
                    SaveCanvasAsSerializedObject(savesFolder);
                }
            }
        }


        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Select a JSON File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                LoadCanvasFromSerializedObject(filePath);
                
            }
        }

        private void ButtonUndo_Click(object sender, RoutedEventArgs e)
        {
            
            if (_focusedShape != null)
            {
                _focusedShape = null;
            }

            if (_historyActions.Count > 0 && _historyActions.Peek().IsUndoable)
            {

                _redoActions.Push(_historyActions.Pop());
                var action = _redoActions.Peek();
                var lastAction = action;
                foreach(Action act in _historyActions)
                {
                    if (act.SHAPE == action.SHAPE)
                    {
                        lastAction = act;
                        break;
                    }

                }
                if (action.IsCreation)
                {
                    action.SHAPE.Visibility = Visibility.Collapsed;
                }
                if(action.IsDeletion)
                {
                    action.SHAPE.Visibility = Visibility.Visible;
                }
                else
                {
                    Canvas.SetLeft(lastAction.SHAPE, lastAction.POINT.X);
                    Canvas.SetTop(lastAction.SHAPE, lastAction.POINT.Y);
                }
            }
        }

        private void ButtonRedo_Click(object sender, RoutedEventArgs e)
        {
            
            if (_focusedShape != null)
            {
                _focusedShape = null;
            }

            if (_redoActions.Count > 0)
            {
                _historyActions.Push(_redoActions.Pop());
                var action = _historyActions.Peek();
                if(action.IsCreation)
                {
                  
                    action.SHAPE.Visibility = Visibility.Visible;
                }
                if (action.IsDeletion)
                {
                    action.SHAPE.Visibility = Visibility.Collapsed;
                }

                Canvas.SetLeft(action.SHAPE, action.POINT.X);
                Canvas.SetTop(action.SHAPE, action.POINT.Y);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if(_focusedShape != null)
            {
                _focusedShape.Visibility = Visibility.Collapsed;
                Action action = new Action(_focusedShape, new Point(Canvas.GetLeft(_focusedShape), Canvas.GetTop(_focusedShape)) , false, true, true);
                Console.WriteLine("Actiune : " + action.SHAPE.ToString() + " " + action.POINT.ToString() + " ");
                _historyActions.Push(action);
                _focusedShape = null;

            }
            else
                MessageBox.Show("Selectati un obiect pentru stergere!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private void ButtonQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }        
        private void AttachEvents(Shape shape)
        {
            if (shape != null)
            {
                shape.KeyDown += A_KeyDown;
                shape.MouseLeftButtonDown += NewShape_MouseLeftButtonDown;
                shape.MouseMove += NewShape_MouseMove;
                shape.MouseLeftButtonUp += NewShape_MouseLeftButtonUp;       

            }

        }

        private Shape CreateShapeCopy(Shape originalShape)
        {
            Shape copy;
            if (originalShape is Rectangle )
            {
                copy = new Rectangle
                {
                    
                    Name = originalShape.Name,
                    Width = originalShape.Width,
                    Height = originalShape.Height,
                    Fill = originalShape.Fill,
                    Focusable = true

                };
            }
            else if (originalShape is Ellipse)
            {
                copy = new Ellipse
                {
                    Name = originalShape.Name,
                    Width = originalShape.Width,
                    Height = originalShape.Height,
                    Fill = originalShape.Fill,
                    Focusable = true
                };
            }
            else
            {
                copy = new Rectangle
                {
                    Name= originalShape.Name,
                    Width = 50,
                    Height = 50,
                    Fill = Brushes.Gray,
                    Focusable = true
                };
            }

            if (copy.Name == masa.Name) {

                _mese.Add(copy);
            }
            canvas.Children.Add(copy);  

            return copy;
        }

        private void LoadCanvasFromSerializedObject(string filePath)
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                var shapeDataList = JsonSerializer.Deserialize<List<ShapeData>>(jsonData);

                if (shapeDataList == null || shapeDataList.Count == 0)
                {
                    MessageBox.Show("Json invalid sau fara valori", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                canvas.Children.Clear();
                ClearAll();
                foreach (var shapeData in shapeDataList)
                {
                    Shape shape = null;

                    switch (shapeData.ShapeType)
                    {
                        case "Rectangle":
                            shape = new Rectangle();
                            break;

                        case "Ellipse":
                            shape = new Ellipse();
                            break;

                        default:
                            MessageBox.Show($"Acest tip de tapa este invalid {shapeData.ShapeType}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                    }

                    if (shape != null)
                    {
                        shape.Width = shapeData.Width;
                        shape.Height = shapeData.Height;

                        if (!string.IsNullOrEmpty(shapeData.FillColor))
                        {
                            shape.Fill = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(shapeData.FillColor));
                        }
                       
                        Point point = new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape));
                       
                        if (point.X < 0)
                            point.X = 0;
                        if (point.Y < 0)
                            point.Y = 0;
                        Action action = new Action(shape, point, false, false, false);
                        _historyActions.Push(action);
                        
                        canvas.Children.Add(shape);
                    }
                    AttachEvents(shape);
                    AddGuideLines(shape);
                    Canvas.SetLeft(shape, shapeData.Left);
                    Canvas.SetTop(shape, shapeData.Top);
                }

                MessageBox.Show("Layout upload succes", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        //Salvare canvas ca layout folosinf JSON
        private void SaveCanvasAsSerializedObject(string folderPath)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filePath = System.IO.Path.Combine(folderPath, $"CanvasData_{timestamp}.json");
                var shapeDataList = new List<ShapeData>();

                foreach (var child in canvas.Children)
                {
                    if(child is not Line)
                    if (child is Shape shape)
                    {
                        string shapeType = shape.GetType().Name;

                        var shapeData = new ShapeData
                        {

                            ShapeType = shapeType,
                            Width = shape.Width,
                            Height = shape.Height,
                            Left = Canvas.GetLeft(shape),
                            Top = Canvas.GetTop(shape),
                            FillColor = (shape.Fill as SolidColorBrush)?.Color.ToString(),

                        };

                        shapeDataList.Add(shapeData);
                    }
                }
                if (shapeDataList.Count == 0)
                {
                    MessageBox.Show("Nu aveti un layout pentru salvare.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string jsonData = JsonSerializer.Serialize(shapeDataList, jsonOptions);

                File.WriteAllText(filePath, jsonData);
                MessageBox.Show($"Layout salvata la {filePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveCanvasAsImage(string folderPath)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(folderPath, $"CanvasImage_{timestamp}.png");
            if (canvas.Children.Count > 0)
            {
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth + 200, (int)canvas.ActualHeight + 200, 96, 96, PixelFormats.Pbgra32);
                renderBitmap.Render(canvas);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(fs);
                }
                MessageBox.Show($"Imagine salvata la {filePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Nu aveti un layout pentru salvare.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public Point GetCoordinatesInHistory(Shape shape)
        {
            foreach (Action act in _historyActions)
            {
                if (act.SHAPE == shape)
                {
                    return new Point(act.POINT.X, act.POINT.Y);
                }

            }
            return new Point(-1, -1);
        }

        public void ClearAll()
        {
            if(_redoActions != null)
                _redoActions.Clear();
            if (_historyActions != null)
                _historyActions.Clear();
            if (_mese != null)
                _mese.Clear();
            if (intersectedVertical != null)
                intersectedVertical.Clear();
            if (intersectedHorizontal != null)
                intersectedHorizontal.Clear();
            if (intersectedRight != null)
                intersectedRight.Clear();
            if (intersectedLeft != null)
                intersectedLeft.Clear();
            if (intersectedTop != null)
                intersectedTop.Clear();
            if (intersectedBottom != null)
                intersectedBottom.Clear();

        }


        private bool IsColliding(Shape movingShape, double newLeft, double newTop)
        {
            Rect movingShapeBounds = new Rect(newLeft, newTop, movingShape.Width, movingShape.Height);

            foreach (UIElement child in canvas.Children)
            {
                if (child is Shape existingShape && existingShape != movingShape)
                {
                    double existingLeft = Canvas.GetLeft(existingShape);
                    double existingTop = Canvas.GetTop(existingShape);
                    Rect existingShapeBounds = new Rect(existingLeft, existingTop, existingShape.Width, existingShape.Height);

                    if (movingShapeBounds.IntersectsWith(existingShapeBounds))
                    {
                        return true; 
                    }
                }
            }

            return false; 
        }


    }

}
