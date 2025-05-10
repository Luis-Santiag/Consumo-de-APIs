using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace otra_poke
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiUrl = "https://pokeapi.co/api/v2/pokemon/";
        public Form1()
        {
            InitializeComponent();
            btnSearch.Click += BtnSearch_Click;
            picPokemon.SizeMode = PictureBoxSizeMode.Zoom;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        //limpiar
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            lblName.Text = "";
            lblHeight.Text = "";
            lblWeight.Text = "";
            lblAbilities.Text = "";
            lblStatus.Text = "";
            picPokemon.Image = null;
        }
        // buscar
        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Buscando...";
            lblStatus.ForeColor = Color.Blue;

            try
            {
                await SearchPokemon(txtSearch.Text.Trim().ToLower());
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private async Task SearchPokemon(string searchTerm)
        {
            try
            {
                // limpiar controles
                picPokemon.Image = null;
                lblName.Text = "";
                lblHeight.Text = "";
                lblWeight.Text = "";
                lblAbilities.Text = "";

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    lblStatus.Text = "Ingresa un nombre o ID de Pokémon";
                    lblStatus.ForeColor = Color.Red;
                    return;
                }

                var response = await _httpClient.GetAsync($"{ApiUrl}{searchTerm}");

                if (!response.IsSuccessStatusCode)
                {
                    lblStatus.Text = "¡Pokémon no encontrado!";
                    lblStatus.ForeColor = Color.Red;
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var pokemon = JsonConvert.DeserializeObject<Pokemon>(content);

                // Mostrar datos
                lblName.Text = char.ToUpper(pokemon.Name[0]) + pokemon.Name.Substring(1);
                lblHeight.Text = $"Altura: {pokemon.Height / 10.0}m";
                lblWeight.Text = $"Peso: {pokemon.Weight / 10.0}kg";
                lblAbilities.Text = "Habilidades: " +
                    string.Join(", ", pokemon.Abilities.Select(a =>
                        char.ToUpper(a.Ability.Name[0]) + a.Ability.Name.Substring(1)));

                // descargar imagen
                if (!string.IsNullOrEmpty(pokemon.Sprites.FrontDefault))
                {
                    
                    try
                    {
                        picPokemon.Image = await DownloadImageAsync(pokemon.Sprites.FrontDefault);
                        picPokemon.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = $"Error al cargar imagen: {ex.Message}";
                        lblStatus.ForeColor = Color.Red;
                    }
                }

                lblStatus.Text = "¡Pokémon encontrado!";
                lblStatus.ForeColor = Color.Green;
            }
            catch (HttpRequestException ex)
            {
                lblStatus.Text = $"Error de conexión: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private async Task<Image> DownloadImageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("No se pudo descargar la imagen");

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                return Image.FromStream(stream);
            }
        }

       // modelos
        public class Pokemon
        {
            public string Name { get; set; }
            public int Height { get; set; }
            public int Weight { get; set; }
            public List<PokemonAbility> Abilities { get; set; }
            public PokemonSprites Sprites { get; set; }
        }

        public class PokemonAbility
        {
            public Ability Ability { get; set; }
        }

        public class Ability
        {
            public string Name { get; set; }
        }

        public class PokemonSprites
        {
            [JsonProperty("front_default")]
            public string FrontDefault { get; set; }
        }
    }
}