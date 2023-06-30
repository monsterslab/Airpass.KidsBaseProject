using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AirpassUnity.VRSports
{
    public class InteractData
    {
        public int lidar;
        public int id;
        public string status;
        public Vector2 position;
        public InteractType type;

        public bool Pressing { get { return (status != "2"); } }

        public InteractData() { }

        public InteractData(int lidar, int _id, string _status, float _x, float _y)
        {
            this.lidar = lidar;
            id = _id;
            status = _status;
            position.x = _x;
            position.y = _y;
        }
    }

    public interface IVRSportsInteractable
    {
        void InteractEnter(InteractData data);
        void InteractDown(InteractData data);
        void InteractHolding(InteractData data);
        void InteractExit(InteractData data);
        void InteractUp(InteractData data);
        void InteractClick(InteractData data);
        //Dictionary<int, InteractData> InteractingData { get; }
    }

    public enum EventType
    {
        None,
        Click,
        Touch,
        Udp,
        NoClick
    }

    public enum InputMode
    {
        none,
        touch,      // Support drag. <Using Unity touch process>.
    }

    public enum InteractType
    {
        enter,
        down,
        holding,
        exit,
        up,
        click
    }

    public enum ButtonType
    {
        enter,
        hold,
        click
    }

    public class InteractableDummy
    {
        public bool isBeenDown = false;
        public InteractData interactData;
        public IVRSportsInteractable interactable;
        public int lostFrame = 0;

        public InteractableDummy(bool isBeenDown, InteractData interactData, IVRSportsInteractable interactable)
        {
            this.isBeenDown = isBeenDown;
            this.interactData = interactData;
            this.interactable = interactable;
            this.lostFrame = 0;
        }
    }
}
