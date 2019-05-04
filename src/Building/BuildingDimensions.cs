using Elements;
using Elements.Geometry;
using System.Collections.Generic;

namespace Bim42HyparQto
{

    /// <summary>
    /// A singelton to regroup all building dimension
    /// </summary>
    public sealed class BuildingDimensions
    {
        private double _width;
        private double _lenght;
        private int _module_number;
        private double _core_width;
        private double _office_space_width;
        private double _corridor_width;
        private double _level_height;
        private double _module_lenght;

        private static readonly BuildingDimensions instance = new BuildingDimensions();

        static BuildingDimensions()
        {
        }

        private BuildingDimensions()
        {
            _module_number = 23;
            _width = 18.5;
            _core_width = 5;
            _corridor_width = 1.5;
            _office_space_width = (_width - _core_width) / 2;
            _level_height = 3.5;
            _module_lenght = 1.35;
            _lenght = _module_number * _module_lenght;
        }

        public static BuildingDimensions Instance
        {
            get
            {
                return instance;
            }
        }
        public int ModuleNumber { get { return _module_number; } set { _module_number = value; } }
        public double Width { get { return _width; } set { _width = value; } }
        public double CoreWidth { get { return _core_width; } set { _core_width = value; } }
        public double OfficeSpaceWidth { get { return _office_space_width; } set { _office_space_width = value; } }
        public double CorridorWidth { get { return _corridor_width; } set { _corridor_width = value; } }
        public double Lenght { get { return _lenght; } set { _lenght = value; } }
        public double LevelHeight { get { return _level_height; } set { _level_height = value; } }

        public double ModuleLenght { get { return _module_lenght; } set { _module_lenght = value; } }

    }

}