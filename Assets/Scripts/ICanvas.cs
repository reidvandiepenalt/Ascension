using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Interface for properties all inventory pages should have
    /// </summary>
    interface ICanvas
    {
        void UpdatedSelection(GameObject newSelection, GameObject oldSelection);

        void Closing();
    }
}
