// Verifica lógica da consulta offline; não substitui inspeção visual no navegador.
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const page = fs.readFileSync(path.join(__dirname, 'avaliar-decks.html'), 'utf8');
const script = page.match(/<script>([\s\S]*)<\/script>/)[1];
const elements = Object.fromEntries(['deck','zone','search','intro','count','cards'].map(id => [id, {value:'',innerHTML:'',textContent:'',children:[],append(o){this.children.push(o);if(!this.value)this.value=o.value;}}]));
elements.zone.value='main';
const document={querySelector:q=>elements[q.slice(1)],createElement:()=>({})};
vm.runInNewContext(script,{document},{timeout:5000});
function check(ok,message){if(!ok)throw new Error(message);}
check(elements.deck.children.length===29,'29 opções de comandante');
for(const option of elements.deck.children){
  elements.deck.value=option.value;
  for(const [zone,total] of [['main',100],['terrains',50]]){
    elements.zone.value=zone;
    elements.deck.onchange();
    check(elements.count.textContent===`${total} cartas exibidas`,option.value+' '+zone);
    check((elements.cards.innerHTML.match(/<article>/g)||[]).length===total,'Quantidade de fichas');
  }
}
elements.search.value='__busca_sem_resultado__';elements.search.oninput();
check(elements.count.textContent==='0 cartas exibidas','Busca vazia');
elements.deck.value='DECK-MED-194';elements.zone.value='main';elements.search.value='goblin';elements.search.oninput();
check(parseInt(elements.count.textContent)>0,'Busca por Goblin');
console.log('Consulta offline: 29 seletores, 58 listas renderizadas em DOM simulado e busca por texto verificados. Inspeção visual não executada.');
