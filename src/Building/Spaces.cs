using System;
using System.Linq;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;


namespace Bim42HyparQto
{
    /// <summary>
    /// The properties of the spaces of the buidings
    /// </summary>
    public class Spaces
    {

        private double _ceiling_void_height;
        private double _ceiling_thickness;
        private double _raised_floor_void_height;
        private double _raised_floor_thickness;
        private double _headroom;
                private FloorType _raisedFloorType;
        private FloorType _ceillingType;
        private WallType _meetingRoomWallType;

        public Spaces(Model model)
        {
            _ceiling_void_height = 0.05;
            _ceiling_thickness = 0.05;
            _raised_floor_void_height = 0.07;
            _raised_floor_thickness = 0.03;
            _headroom = 2.7;

                        List<MaterialLayer> raisedFloorMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Wood,_raised_floor_thickness)
            };
            _raisedFloorType = new FloorType("Raised floor", raisedFloorMaterial);

            List<MaterialLayer> ceillingMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Default,_ceiling_thickness)
            };

            _ceillingType = new FloorType("Ceilling", ceillingMaterial);

            //Create walls types
            List<MaterialLayer> meetingRoomMaterialLayers = new List<MaterialLayer>(){
                new MaterialLayer(BuiltInMaterials.Glass, 0.1)
            };
            _meetingRoomWallType = new WallType("Meeting Room Wall", meetingRoomMaterialLayers);

        }
        public double CeilingVoidHeight { get { return _ceiling_void_height; } set { _ceiling_void_height = value; } }
        public double CeilingThickness { get { return _ceiling_thickness; } set { _ceiling_thickness = value; } }
        public double RaisedFloorVoidHeight { get { return _raised_floor_void_height; } set { _raised_floor_void_height = value; } }
        public double RaisedFloorThickness { get { return _raised_floor_thickness; } set { _raised_floor_thickness = value; } }
        public double Headroom { get { return _headroom; } set {  _headroom = value; } }

        //         public FloorType RaisedFloorType { get { return raisedFloorType; } }
        // public FloorType CeillingType { get { return ceillingType; } }
        
        // public WallType MeetingRoomWallType { get { return meetingRoomWallType; } }
    }
}