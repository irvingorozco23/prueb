using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace prueb
{
    public partial class Form1 : Form
    {
        // Ajusta la cadena de conexión a tu base de datos MySQL
        string sqlconnection = "Server=localhost; Port=3306; Database=prueba; UID=root; Pwd=2312;";

        public Form1()
        {
            InitializeComponent();
            // Asegúrate de que el DataGridView tenga las columnas necesarias configuradas en el diseñador
            // incluyendo una columna llamada "cantidad"
        }

        private void textBoxCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string code_b = textBoxCodigo.Text.Trim();
                textBoxCodigo.Clear(); // Limpiar el TextBox
                CargarProducto(code_b);
            }
        }
        private void btnCobrar_Click(object sender, EventArgs e)
        {
           decimal total = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["precio"].Value != null && row.Cells["cantidad"].Value != null)
                {
                    decimal precio = Convert.ToDecimal(row.Cells["precio"].Value);
                    int cantidad = Convert.ToInt32(row.Cells["cantidad"].Value);
                    decimal subtotal = precio * cantidad;
                    total += subtotal;

                    // Opcional: Mostrar el subtotal para cada producto
                    row.Cells["subtotal"].Value = subtotal; // Asegúrate de tener una columna "subtotal" en tu DataGridView
                }
            }

            // Verificar si se ingresó un monto de pago válido
            if (decimal.TryParse(tbmonto.Text, out decimal montoPago))
            {
                if (montoPago >= total)
                {
                    decimal cambio = montoPago - total;
                    MessageBox.Show($"Total a pagar: ${total:F2}\n" +
                                    $"Monto pagado: ${montoPago:F2}\n" +
                                    $"Cambio: ${cambio:F2}");

                    // Opcional: Limpiar el DataGridView y el TextBox después de cobrar
                    dataGridView1.Rows.Clear();
                    tbmonto.Clear();
                }
                else
                {
                    MessageBox.Show($"El monto pagado (${montoPago:F2}) es insuficiente.\n" +
                                    $"Total a pagar: ${total:F2}");
                }
            }
            else
            {
                MessageBox.Show("Por favor, ingrese un monto de pago válido.");
            }
        }


        private void CargarProducto(string code_b)
        {
            using (MySqlConnection connection = new MySqlConnection(sqlconnection))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT code_b, nombre, precio, caducidad FROM productos WHERE code_b = @code_b";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@code_b", code_b);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow row = dataTable.Rows[0];
                        bool productoExistente = false;

                        foreach (DataGridViewRow dgvRow in dataGridView1.Rows)
                        {
                            if (dgvRow.Cells["code_b"].Value != null &&
                                dgvRow.Cells["code_b"].Value.ToString() == row["code_b"].ToString())
                            {
                                int cantidad = Convert.ToInt32(dgvRow.Cells["cantidad"].Value) + 1;
                                dgvRow.Cells["cantidad"].Value = cantidad;

                                // Calcular y actualizar el subtotal
                                decimal precio = Convert.ToDecimal(dgvRow.Cells["precio"].Value);
                                dgvRow.Cells["subtotal"].Value = precio * cantidad;

                                productoExistente = true;
                                break;
                            }
                        }

                        if (!productoExistente)
                        {
                            decimal precio = Convert.ToDecimal(row["precio"]);
                            int cantidad = 1;
                            decimal subtotal = precio * cantidad;

                            dataGridView1.Rows.Add(
                                row["code_b"],
                                row["nombre"],
                                precio,
                                row["caducidad"],
                                cantidad,
                                subtotal
                            );
                        }

                        dataGridView1.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró ningún producto con ese código de barras.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar el producto: " + ex.Message);
                }
            }
        }
    }
}