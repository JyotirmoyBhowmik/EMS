import { useEffect, useRef } from 'react';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';

interface DigitalTwinProps {
    equipmentType: 'turbine' | 'solar' | 'battery';
    realTimeData: {
        [key: string]: number;
    };
}

export default function DigitalTwin({ equipmentType, realTimeData }: DigitalTwinProps) {
    const containerRef = useRef<HTMLDivElement>(null);
    const sceneRef = useRef<THREE.Scene | null>(null);
    const rendererRef = useRef<THREE.WebGLRenderer | null>(null);

    useEffect(() => {
        if (!containerRef.current) return;

        // Scene setup
        const scene = new THREE.Scene();
        scene.background = new THREE.Color(0x0f172a);
        sceneRef.current = scene;

        // Camera
        const camera = new THREE.PerspectiveCamera(
            75,
            containerRef.current.clientWidth / containerRef.current.clientHeight,
            0.1,
            1000
        );
        camera.position.z = 5;

        // Renderer
        const renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setSize(containerRef.current.clientWidth, containerRef.current.clientHeight);
        containerRef.current.appendChild(renderer.domElement);
        rendererRef.current = renderer;

        // Controls
        const controls = new OrbitControls(camera, renderer.domElement);
        controls.enableDamping = true;

        // Lighting
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.5);
        scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(5, 5, 5);
        scene.add(directionalLight);

        // Create 3D model based on equipment type
        let model: THREE.Group;
        switch (equipmentType) {
            case 'turbine':
                model = createWindTurbine();
                break;
            case 'solar':
                model = createSolarPanel();
                break;
            case 'battery':
                model = createBattery();
                break;
        }
        scene.add(model);

        // Animation loop
        const animate = () => {
            requestAnimationFrame(animate);

            // Rotate model
            model.rotation.y += 0.005;

            // Update based on real-time data
            updateModelWithData(model, realTimeData);

            controls.update();
            renderer.render(scene, camera);
        };
        animate();

        // Cleanup
        return () => {
            containerRef.current?.removeChild(renderer.domElement);
            renderer.dispose();
        };
    }, [equipmentType]);

    // Update model when data changes
    useEffect(() => {
        if (sceneRef.current) {
            const model = sceneRef.current.children.find((child) => child instanceof THREE.Group);
            if (model) {
                updateModelWithData(model as THREE.Group, realTimeData);
            }
        }
    }, [realTimeData]);

    return (
        <div ref={containerRef} className="w-full h-96 rounded-lg overflow-hidden" />
    );
}

function createWindTurbine(): THREE.Group {
    const group = new THREE.Group();

    // Tower
    const towerGeometry = new THREE.CylinderGeometry(0.1, 0.15, 3, 32);
    const towerMaterial = new THREE.MeshStandardMaterial({ color: 0xcccccc });
    const tower = new THREE.Mesh(towerGeometry, towerMaterial);
    tower.position.y = 0;
    group.add(tower);

    // Nacelle
    const nacelleGeometry = new THREE.BoxGeometry(0.5, 0.3, 0.8);
    const nacelleMaterial = new THREE.MeshStandardMaterial({ color: 0xeeeeee });
    const nacelle = new THREE.Mesh(nacelleGeometry, nacelleMaterial);
    nacelle.position.y = 1.5;
    group.add(nacelle);

    // Blades
    const bladesGroup = new THREE.Group();
    for (let i = 0; i < 3; i++) {
        const bladeGeometry = new THREE.BoxGeometry(0.1, 1.5, 0.05);
        const bladeMaterial = new THREE.MeshStandardMaterial({ color: 0x3b82f6 });
        const blade = new THREE.Mesh(bladeGeometry, bladeMaterial);
        blade.position.y = 0.75;
        blade.rotation.z = (i * Math.PI * 2) / 3;
        bladesGroup.add(blade);
    }
    bladesGroup.position.set(0, 1.5, 0.5);
    bladesGroup.name = 'blades'; // For animation
    group.add(bladesGroup);

    return group;
}

function createSolarPanel(): THREE.Group {
    const group = new THREE.Group();

    // Panel
    const panelGeometry = new THREE.BoxGeometry(2, 0.05, 1.2);
    const panelMaterial = new THREE.MeshStandardMaterial({
        color: 0x1e3a8a,
        metalness: 0.8,
        roughness: 0.2
    });
    const panel = new THREE.Mesh(panelGeometry, panelMaterial);
    panel.name = 'panel';
    group.add(panel);

    // Grid lines
    const linesMaterial = new THREE.LineBasicMaterial({ color: 0xffffff });
    for (let i = -0.9; i <= 0.9; i += 0.3) {
        const points = [new THREE.Vector3(i, 0.03, -0.6), new THREE.Vector3(i, 0.03, 0.6)];
        const geometry = new THREE.BufferGeometry().setFromPoints(points);
        const line = new THREE.Line(geometry, linesMaterial);
        group.add(line);
    }

    // Support
    const supportGeometry = new THREE.CylinderGeometry(0.05, 0.05, 1, 16);
    const supportMaterial = new THREE.MeshStandardMaterial({ color: 0x666666 });
    const support = new THREE.Mesh(supportGeometry, supportMaterial);
    support.position.y = -0.5;
    group.add(support);

    return group;
}

function createBattery(): THREE.Group {
    const group = new THREE.Group();

    // Battery body
    const bodyGeometry = new THREE.CylinderGeometry(0.5, 0.5, 1.5, 32);
    const bodyMaterial = new THREE.MeshStandardMaterial({ color: 0x10b981 });
    const body = new THREE.Mesh(bodyGeometry, bodyMaterial);
    body.name = 'body';
    group.add(body);

    // Positive terminal
    const terminalGeometry = new THREE.CylinderGeometry(0.15, 0.15, 0.2, 16);
    const terminalMaterial = new THREE.MeshStandardMaterial({ color: 0xfbbf24 });
    const terminal = new THREE.Mesh(terminalGeometry, terminalMaterial);
    terminal.position.y = 0.85;
    group.add(terminal);

    // Charge level indicator
    const levelGeometry = new THREE.BoxGeometry(1.1, 0.3, 0.1);
    const levelMaterial = new THREE.MeshStandardMaterial({
        color: 0x22c55e,
        transparent: true,
        opacity: 0.7
    });
    const level = new THREE.Mesh(levelGeometry, levelMaterial);
    level.position.y = 0;
    level.name = 'chargeLevel';
    group.add(level);

    return group;
}

function updateModelWithData(model: THREE.Group, data: { [key: string]: number }) {
    // Update turbine blade rotation based on wind speed
    const blades = model.getObjectByName('blades');
    if (blades && data.windSpeed) {
        blades.rotation.z = Date.now() * 0.001 * (data.windSpeed / 10);
    }

    // Update solar panel color based on power output
    const panel = model.getObjectByName('panel');
    if (panel && data.powerOutput) {
        const intensity = Math.min(data.powerOutput / 1000, 1);
        (panel as THREE.Mesh).material = new THREE.MeshStandardMaterial({
            color: new THREE.Color(0.2 * intensity, 0.4 * intensity, 0.8),
            metalness: 0.8,
            roughness: 0.2
        });
    }

    // Update battery color based on charge level
    const body = model.getObjectByName('body');
    const chargeLevel = model.getObjectByName('chargeLevel');
    if (body && data.stateOfCharge) {
        const charge = data.stateOfCharge / 100;
        (body as THREE.Mesh).material = new THREE.MeshStandardMaterial({
            color: new THREE.Color(
                1 - charge,  // Red when low
                charge,      // Green when high
                0.3
            )
        });

        // Update charge level indicator
        if (chargeLevel) {
            chargeLevel.scale.y = charge;
        }
    }
}
