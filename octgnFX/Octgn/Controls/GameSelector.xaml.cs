using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using Octgn.DataNew.Entities;
using Octgn.DataNew;
using System.Linq;
using Octgn.Core.DataExtensionMethods;
using Octgn.Extentions;
using Microsoft.Scripting.Utils;

namespace Octgn.Controls
{
    public sealed partial class GameSelector : UserControl
    {
        public event EventHandler<Game> GameChanged;

        private int selectedIdx = 0;

        private Game[] _games;

        private static readonly Duration dt = new Duration(TimeSpan.FromMilliseconds(500));

        public GameSelector() {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Loaded += GameSelector_Loaded;
        }

        private void GameSelector_Loaded(object sender, RoutedEventArgs e) {
            var context = DbContext.Get();

            _games = context.Games.ToArray();

            Create3DItems();
        }

        public Game Game {
            get {
                Dispatcher.VerifyAccess();

                if (_games.Length == 0) return null;

                return _games[selectedIdx];
            }
        }

        private void Create3DItems() {
            container.Children.Clear();

            var i = 0;

            foreach (var game in _games) {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;

                var cardSize = game.DefaultSize();

                bmp.UriSource = new Uri(cardSize.Back);
                bmp.EndInit();

                var halfWidth = (cardSize.BackWidth * 1.5 / cardSize.BackHeight) * 2;
                var item = new ModelUIElement3D() {
                    Model = new GeometryModel3D() {
                        Geometry = new MeshGeometry3D() {
                            Positions = new Point3DCollection()
                                    {
                                    new Point3D(-halfWidth,0,-1),
                                    new Point3D(-halfWidth,6,-1),
                                    new Point3D(halfWidth,6,-1),
                                    new Point3D(halfWidth,0,-1)
                                },
                            TriangleIndices = new Int32Collection() { 0, 2, 1, 3, 2, 0 },
                            TextureCoordinates = new PointCollection() { new Point(0, 1), new Point(0, 0), new Point(1, 0), new Point(1, 1) }
                        },
                        Material = new DiffuseMaterial(new ImageBrush(bmp)),
                        Transform = new Transform3DGroup() {
                            Children = new Transform3DCollection()
                                    {
                                    new RotateTransform3D()
                                    {
                                        Rotation = new AxisAngleRotation3D(new Vector3D(0,1,0), i == 0 ? 0 : -40)
                                    },
                                    new TranslateTransform3D(i == 0 ? 0 : 0.5+i, 0, i != 0 ? -1 : 0)
                                }
                        }
                    }
                };
                item.MouseDown += Select;
                container.Children.Add(item);
                ++i;
            }

            if (_games.Length > 0) {
                selectedIdx = 0;

                GameChanged?.Invoke(this, Game);
            }
        }

        private void Select(object sender, RoutedEventArgs e) {
            e.Handled = true;
            var idx = container.Children.IndexOf((Visual3D)sender);
            Select(idx);
        }
        private void Select(int idx) {
            if (idx == selectedIdx) return;
            for (int i = 0; i < container.Children.Count; ++i) {
                var item = (ModelUIElement3D)container.Children[i];
                var rotate = (RotateTransform3D)((Transform3DGroup)item.Model.Transform).Children[0];
                var translate = (TranslateTransform3D)((Transform3DGroup)item.Model.Transform).Children[1];

                var anim = new DoubleAnimation(i == idx ? 0 : -1, dt, FillBehavior.HoldEnd) { DecelerationRatio = 0.65 };
                translate.BeginAnimation(TranslateTransform3D.OffsetZProperty, anim, HandoffBehavior.SnapshotAndReplace);
                anim = new DoubleAnimation(i - idx + 0.5 * Math.Sign(i - idx), dt, FillBehavior.HoldEnd) { DecelerationRatio = 0.65 };
                translate.BeginAnimation(TranslateTransform3D.OffsetXProperty, anim, HandoffBehavior.SnapshotAndReplace);
                anim = new DoubleAnimation(i < idx ? 40 : i > idx ? -40 : 0, dt, FillBehavior.HoldEnd) { DecelerationRatio = 0.65 };
                ((AxisAngleRotation3D)rotate.Rotation).BeginAnimation(AxisAngleRotation3D.AngleProperty, anim, HandoffBehavior.SnapshotAndReplace);
            }
            selectedIdx = idx;
            GameChanged?.Invoke(this, Game);
        }

        public void Select(Guid? gameId) {
            if (_games == null) return;

            if (_games.Length == 0) return;

            int selectIndex;
            if (gameId == null) {
                if (selectedIdx == 0) return;

                selectIndex = 0;
            } else {
                var gameIndex = _games.FindIndex(g => g.Id == gameId);

                if (gameIndex == -1) gameIndex = 0;

                if (gameIndex == selectedIdx) return;

                selectIndex = gameIndex;
            }

            Select(selectIndex);
        }
    }
}