// No navegador nao ha react-native-maps: mostra um aviso e a lista continua disponivel.
import { View } from 'react-native';

import { espaco, useTema } from '@/theme';

import { Texto } from './base';
import type { PropsMapa } from './Mapa';

export function Mapa(_: PropsMapa) {
  const { c } = useTema();
  return (
    <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center', padding: espaco[6], backgroundColor: c.bgSubtle }}>
      <Texto cor={c.textMuted} style={{ textAlign: 'center' }}>
        O mapa aparece no celular. Use a opção Lista.
      </Texto>
    </View>
  );
}
