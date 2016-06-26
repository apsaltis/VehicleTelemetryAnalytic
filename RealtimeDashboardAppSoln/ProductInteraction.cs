using System;

namespace RealtimeDashboardApp
{
  
    public class ProductInteraction
    {
    
      
        public int InteractionID { get; set; }
        public int ProductID { get; set; }
        //public string ProductName { get; set { 
        //                                        switch (ProductID) { 
        //                                         case 33:
        //                                            ProductName= "Furniture";
        //                                            break;
        //                                         case 213:
        //                                            ProductName = "Rugs";
        //                                            break;
        //                                         case 211:
        //                                            ProductName = "Pillows";
        //                                            break;
        //                                         case 9:
        //                                            ProductName = "Cocktail Glasses";
        //                                            break;
        //                                         case 51:
        //                                            ProductName = "Candles";
        //                                            break;
        //                                         };
        //                                    } 
        //                           }
        public string CustomerID { get; set; }
        public DateTime InteractionTime { get; set; }
        public int ix { get; set; }
        public int iy { get; set; }
        public int InteractionLength { get; set; }
    }
}