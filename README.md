# Metroidvania

Demo 2D de plataformas creada con Unity, con movimiento de personaje, combate contra enemigos y pantallas de victoria o derrota.

## Requisitos

- Unity `6000.0.73f1`.
- Los paquetes del proyecto se restauran desde `Packages/manifest.json`, incluyendo URP 2D, Cinemachine y el Input System.

## Ejecutar la demo

1. Abre el proyecto desde Unity Hub con la version indicada.
2. En la ventana **Project**, abre la escena:

   `Assets/_Project/Scenes/Test/Template1.unity`

3. Pulsa **Play** en el editor.

Esta es la escena de prueba funcional. Incluye el nivel de tilemaps, el jugador, enemigos, `GameManager`, la camara y los paneles de victoria y derrota.

> Nota: antes de crear una compilacion, agrega `Assets/_Project/Scenes/Test/Template1.unity` a **File > Build Profiles > Scenes In Build**. La configuracion actual de escenas conserva referencias antiguas que no existen en el proyecto.

## Controles de la demo

| Accion | Tecla |
| --- | --- |
| Moverse | `A` / `D` o flechas izquierda / derecha |
| Saltar, doble salto y salto de pared | `Z` |
| Dash | `C` |
| Ataque cuerpo a cuerpo | `X` |
| Lanzar arma | `V` |

Los controles de esta escena estan implementados con el Input Manager clasico de Unity en los scripts del jugador. El asset `Assets/InputSystem_Actions.inputactions` permanece disponible para una futura migracion al nuevo Input System.

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