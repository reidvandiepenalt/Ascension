using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    interface ICanvas
    {
        void UpdatedSelection(GameObject newSelection, GameObject oldSelection);

        void Closing();
    }
}
