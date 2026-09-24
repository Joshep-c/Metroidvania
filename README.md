# Metroidvania

Demo 2D de plataformas creada con Unity, con movimiento de personaje, combate contra enemigos y pantallas de victoria o derrota.

## Requisitos

- Unity `6000.6.1f1`.
- Los paquetes del proyecto se restauran desde `Packages/manifest.json`, incluyendo URP 2D, Cinemachine y el Input System.

## Ejecutar la demo

1. Abre el proyecto desde Unity Hub con la version indicada.
2. En la ventana **Project**, abre la escena:

   `Assets/_Project/Scenes/Test/Template1.unity`

3. Pulsa **Play** en el editor.

Esta es la escena de prueba funcional. Incluye el nivel de tilemaps, el jugador, enemigos, `GameManager`, la camara, un HUD con vida y enemigos restantes, un limite inferior para detectar caidas y los paneles de victoria y derrota.

La escena ya esta incluida en **Scenes In Build**, por lo que tambien se puede reiniciar desde las pantallas de victoria y derrota en una compilacion.

## Controles de la demo

| Accion | Teclado | Mando |
| --- | --- | --- |
| Moverse | `A` / `D` o flechas izquierda / derecha | Stick izquierdo o cruceta |
| Saltar, doble salto y salto de pared | `Z` o espacio | Boton inferior |
| Dash | `C` | Boton superior derecho (RB/R1) |
| Ataque cuerpo a cuerpo | `X` | Boton izquierdo |
| Lanzar arma | `V` | Boton derecho |

Los controles del jugador usan `Assets/InputSystem_Actions.inputactions` mediante el nuevo Input System. El stick izquierdo tiene zona muerta para evitar desplazamientos accidentales.

## Estructura relevante

| Ruta | Contenido |
| --- | --- |
| `Assets/_Project/Scenes/Test/` | Escena de prueba de la demo |
| `Assets/_Project/Scripts/Player/` | Movimiento, controlador, camara, ataque y arma arrojadiza |
| `Assets/_Project/Scripts/Enemies/` | Enemigos y proyectiles |
| `Assets/_Project/Scripts/System/` | Gestion de partida, zonas de muerte y objetos del escenario |
| `Assets/_Project/Prefabs/` | Prefabs del juego |
| `Assets/_Project/Art/` | Materiales y recursos visuales |

## Flujo de juego

El jugador recorre el escenario, evita peligros y derrota enemigos. Al eliminar a todos los enemigos registrados, `GameManager` muestra la pantalla de victoria. Si la vida del jugador llega a cero, muestra la pantalla de derrota; ambos estados detienen el tiempo de juego y permiten reiniciar el nivel desde la interfaz.
