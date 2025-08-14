using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SGT_BRIDGE.Models.Order
{
    public class OrderProduct
    {
        int? _position;
        /// <summary>
        /// Nr pozycji w zamówieniu. Brak oznacza pozycję automatyczną
        /// </summary>
        public int? Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        int _quantity;
        /// <summary>
        /// Ilość produktu w zamówieniu
        /// </summary>
        [Required]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Product quantity must be greater than zero");

                _quantity = value;
            }
        }

        string _code = "";
        /// <summary>
        /// Sku lub Ean produktu
        /// </summary>
        [Required]
        public string Code
        {
            get => _code;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentException("Produkt musi posiadać kod (Sku lub Ean)");

                _code = value;
            }
        }

        string _name = "";
        /// <summary>
        /// Nazwa produktu
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (value == null)
                    throw new ArgumentException("Product name cannot be empty");

                _name = value;
            }
        }

        decimal? _price_brutto;
        /// <summary>
        /// Cena brutto na zamówieniu. Jeśli pusta - zostanie nadpisana przy dodawaniu
        /// </summary>
        public decimal? Price
        {
            get => _price_brutto;
            set
            {
                if (value != null && value.Value < 0)
                {
                    throw new ArgumentException("Product price cannot be lower than zero");
                }

                _price_brutto = value;
            }
        }

        decimal? _price_drop = 0.0m;

        /// <summary>
        /// Rabat od ceny brutto
        /// </summary>
        public decimal? Price_drop
        {
            get => _price_drop;
            set
            {
                if (value >= 100)
                {
                    throw new ArgumentException("Product's price drop can not be greater or equal 100%");
                }

                _price_drop = value;
            }
        }

        string _description = "";
        /// <summary>
        /// Opis pozycji<br/>
        /// Np. Rabat na pierwsze zamówienie 5%
        /// </summary>
        public string Descrption
        {
            get
            {
                return _description;
            }
            set
            {
                if (value != null && value.Length > 255)
                    throw new ArgumentException($"OrderItem description can not be longer than 255 characters ({value.Length} occured)");

                _description = value;
            }
        }

        public OrderProduct() { }

        public OrderProduct(int qty, string code, decimal? price = null, decimal? price_drop = null, int? position = null, string descrption = "")
        {
            Code = code;
            Name = "";
            Quantity = qty;
            Price = price;
            Price_drop = price_drop;
            Position = position;
            Descrption = descrption;
        }

        public override string ToString()
        {
            string pos = "";
            if (Position.HasValue && Position.Value > 0)
            {
                pos = $"pos={Position}, ";
            }

            return $"Product({pos}Code={Code}, cnt={Quantity})";
        }
    }
}
